using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

public partial class BloodSimCPU : Node2D
{
    [Export]
    MultiMeshInstance2D Display;
    [Export]
    uint MaxEnemyCount = 1000;
    [Export]
    uint MaxParticleCount = 10;
    [Export]
    uint MaxMomentaryInstantiationPositions = 10;
    // first ind is the sum, subsequent indexes are the amount of particles instantiated at the corresponding position minus 1 in InstantiatePosition
    // done this way so its easy to update the buffer ᓚᘏᗢ
    int[] ToInstantiate; 
    Vector2[] InstantiatePosition;
    int InstantiateInd = 0;
    [Export]
    ImageTexture[] samplePatterns = new ImageTexture[16];
    [Export]
    uint MaxFieldCount = 1000;
    RenderingDevice Renderer = RenderingServer.CreateLocalRenderingDevice();

    enum ByteCount
    {
        glvec2 = 8,
        glvec4 = 16,
        glfloat = 4,
        glbool = 4,
        glint = 4,

    }
    // Called when the node enters the scene tree for the first time.
    Rid Shader;
    Rid Pipeline;
    Rid UniformSet;
    long Computelist;
    struct enemyStruct
    {
        public Rid Position;
        public Rid Radius;
        public Rid Damage;
    }
    enemyStruct Enemy;
    struct particleStruct
    {
        public Rid Position;
        public Rid Velocity;
        public Rid InUse;
        public Rid ToInstantiate;
        public Rid InstantiatePosition;
        public Rid Misc;
    }
    particleStruct Particle;
    struct playerStruct
    {
        public Rid FieldPosition;
        public Rid FieldVelocity;
        public Rid FieldMagnitude;
        public Rid PatternIndex;
        public Rid FieldSize;
    }
    playerStruct PlayerInput;
    Rid[] Sampler = new Rid[16];
    Rid[] Patterns = new Rid[16];


    Rid CompileShader(String file)
    {
        RDShaderFile shaderFile = GD.Load<RDShaderFile>(file);
        return Renderer.ShaderCreateFromSpirV(shaderFile.GetSpirV());
    }
    byte[] ToByteArray<E>(E[] data, ByteCount byteCountOfType)
    {

        byte[] input = new byte[data.Length * (int)byteCountOfType];
        Buffer.BlockCopy(data, 0, input, 0, input.Length);
        return input;
    }
    float[] Vec2ToFloatArray(Vector2[] input)
    {
        float[] array = new float[input.Length*2];
        for (int i = 0; i < input.Length; i++)
        {
            array[i * 2] = input[i].X;
            array[i * 2+1] = input[i].Y;
        }
        return array;
    }

    int bindingInd = 0;
    RDUniform NewUniform()
    {
        return new RDUniform()
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = bindingInd++
        };
    }
    RDUniform NewUniform(Rid init)
    {
        RDUniform a = NewUniform();
        a.AddId(init);
        return a;
    }
    Rid NewStorageBuffer(byte[] input) {
        return Renderer.StorageBufferCreate((uint)input.Length, input);
    }
    Godot.Collections.Array<RDUniform> EnemyData()
    {

        Godot.Collections.Array<RDUniform> data = new Godot.Collections.Array<RDUniform>();
        data.Add(NewUniform(Enemy.Position = NewStorageBuffer(new byte[MaxEnemyCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(Enemy.Radius   = NewStorageBuffer(new byte[MaxEnemyCount * (int)ByteCount.glfloat])));
        data.Add(NewUniform(Enemy.Damage   = NewStorageBuffer(new byte[MaxEnemyCount * (int)ByteCount.glint])));
		return data;
	}
    Godot.Collections.Array<RDUniform> ParticleData()
    {

        ToInstantiate = new int[1+MaxMomentaryInstantiationPositions];
        InstantiatePosition = new Vector2[MaxMomentaryInstantiationPositions*2];

        Godot.Collections.Array<RDUniform> data = new Godot.Collections.Array<RDUniform>();
        data.Add(NewUniform(Particle.Position            = NewStorageBuffer(new byte[MaxParticleCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(Particle.Velocity            = NewStorageBuffer(new byte[MaxParticleCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(Particle.InUse               = NewStorageBuffer(new byte[MaxParticleCount * (int)ByteCount.glbool])));
        data.Add(NewUniform(Particle.ToInstantiate       = NewStorageBuffer(new byte[(int)ByteCount.glint + ToInstantiate.Length * (int)ByteCount.glint])));
        data.Add(NewUniform(Particle.InstantiatePosition = NewStorageBuffer(new byte[InstantiatePosition.Length * (int)ByteCount.glvec2])));
        data.Add(NewUniform(Particle.Misc                = NewStorageBuffer(new byte[(int)ByteCount.glfloat + MaxParticleCount * (int)ByteCount.glfloat])));
        return data;
    }
    Godot.Collections.Array<RDUniform> PlayerData()
    {
        Godot.Collections.Array<RDUniform> data = new Godot.Collections.Array<RDUniform>();
        data.Add(NewUniform(PlayerInput.FieldPosition  = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(PlayerInput.FieldVelocity  = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(PlayerInput.FieldMagnitude = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glfloat])));
        data.Add(NewUniform(PlayerInput.PatternIndex   = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glint])));
        data.Add(NewUniform(PlayerInput.FieldSize      = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glfloat])));
        return data;
    }
    Rid GetData()
	{
        Godot.Collections.Array < RDUniform > data = new Godot.Collections.Array<RDUniform> ();
        bindingInd = 0;
		data.AddRange(EnemyData());
        data.AddRange(ParticleData());
        data.AddRange(PlayerData());
        RDTextureFormat format = new RDTextureFormat()
        {
            Width = (uint)128,//samplePatterns[i].GetWidth(),
            Height = (uint)128,//samplePatterns[i].GetHeight(),
            Depth = 16,
            TextureType = RenderingDevice.TextureType.Type2DArray,
            Format = RenderingDevice.DataFormat.R4G4B4A4UnormPack16,
            UsageBits = RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.SamplingBit,
        };


        RDUniform patternUniform = new RDUniform
        {
            UniformType = RenderingDevice.UniformType.SamplerWithTexture,
            Binding = bindingInd++
        };
        RDSamplerState samplerState = new RDSamplerState();
        for (int i = 0; i < 16; i++)
        {
            Sampler[i] = Renderer.SamplerCreate(samplerState);
            Patterns[i] = Renderer.TextureCreate(format, new RDTextureView());
            patternUniform.AddId(Sampler[i]);
            patternUniform.AddId(Patterns[i]);
        }

        data.Add(patternUniform);

        return Renderer.UniformSetCreate(data, Shader,0);
	}

    void InitializeCompute()
	{
		Pipeline = Renderer.ComputePipelineCreate(Shader);
        UniformSet = GetData();
    }
    void RunCompute()
    {
        Computelist = Renderer.ComputeListBegin();
        Renderer.ComputeListBindComputePipeline(Computelist, Pipeline);
        Renderer.ComputeListBindUniformSet(Computelist,UniformSet, 0);
        Renderer.ComputeListDispatch(Computelist, xGroups: MaxParticleCount, yGroups: 1, zGroups: 1);
        Renderer.ComputeListEnd();
        Renderer.Submit();
    }

    void ProcessOutputDamageData(byte[] array)
    {
        int[] intArray = new int[array.Length / (int) ByteCount.glint];
        Buffer.BlockCopy(array, 0, intArray, 0, array.Length);
        for (int i = 0; i < HasHP.EntityList.Count; i++)
        {
            HasHP node = HasHP.EntityList[i];
            if (intArray[i] > 0)
                node.HP.TakeDamage(intArray[i]);
        }
    }

    void UpdateEnemyData(double delta)
    {
        //ProcessOutputDamageData(Renderer.BufferGetData(Enemy.Damage));
        Renderer.BufferGetDataAsync(Enemy.Damage, Callable.From<byte[]>(ProcessOutputDamageData));
        float[] positionData = new float[HasHP.EntityList.Count*2];
        float[] radiusData = new float[HasHP.EntityList.Count];
        int[] outputDamageData = new int[HasHP.EntityList.Count];
        for (int i = 0; i < HasHP.EntityList.Count; i++)
        {
            HasHP node = HasHP.EntityList[i];
            positionData[i*2] = (node as Node2D).Position.X;
            positionData[i*2+1] = (node as Node2D).Position.Y;
            radiusData[i] = node.ParticleHitboxRadius;
        }
        byte[] positionDataBytes     = ToByteArray(positionData,     ByteCount.glfloat);
        byte[] radiusDataBytes       = ToByteArray(radiusData,       ByteCount.glfloat);
        byte[] outputDamageDataBytes = ToByteArray(outputDamageData, ByteCount.glint);
        Renderer.BufferUpdate(Enemy.Position, 0, (uint)positionDataBytes.Length    , positionDataBytes);
        Renderer.BufferUpdate(Enemy.Radius  , 0, (uint)radiusDataBytes.Length      , radiusDataBytes);
        Renderer.BufferUpdate(Enemy.Damage  , 0, (uint)outputDamageDataBytes.Length, outputDamageDataBytes);
        
    }

    void InstantiateParticles(int count, Vector2 position)
    {
        ToInstantiate[InstantiateInd+1] = count;
        InstantiatePosition[InstantiateInd] = position;
        ToInstantiate[0] += count;
        InstantiateInd++;
    }

    void UpdateParticleData(double delta)
    {
        GD.Print("called");
        byte[] ToInstantiateBytes = ToByteArray(ToInstantiate, ByteCount.glint);
        byte[] InstantiatePositionBytes = ToByteArray(Vec2ToFloatArray(InstantiatePosition), ByteCount.glfloat);
        //GD.Print(ToInstantiate[0]);
        //float[] test = new float[InstantiatePosition.Length];
        //Buffer.BlockCopy(InstantiatePositionBytes, 0, test, 0, InstantiateInd * (int)ByteCount.gdvec2);
        //GD.Print(String.Join(",", test));
        Renderer.BufferUpdate(Particle.ToInstantiate      , 0, (uint)(1+InstantiateInd)*(uint)ByteCount.glint, ToInstantiateBytes);
        Renderer.BufferUpdate(Particle.InstantiatePosition, 0, (uint)InstantiateInd * (uint)ByteCount.glvec2, InstantiatePositionBytes);

        InstantiateInd = 0;
        ToInstantiate[0] = 0;

        Renderer.BufferUpdate(Particle.Misc, 0, (uint)ByteCount.glfloat, BitConverter.GetBytes((float)delta));
    }
    void UpdatePlayerData(double delta)
    {
        //TODO patterns
    }
    void UpdateSimulation(double delta)
    {
        UpdateEnemyData(delta);
        UpdateParticleData(delta);
        UpdatePlayerData(delta);
        RunCompute();
    }
    public override void _Ready()
	{
		Shader = CompileShader("res://hemomancy/bloodsim.glsl");
        InitializeCompute();
        Renderer.Submit();

    }
    String bufferToString<e>(Rid buffer, ByteCount type)
    {
        byte[] array = Renderer.BufferGetData(buffer);
        e[] vecArray = new e[array.Length / (int)type];
        Buffer.BlockCopy(array, 0, vecArray, 0, array.Length);
        return String.Join(",", vecArray);

    }
    double timer = 0;
    double interval = 1/24f;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        timer += delta;
        if (timer > interval)
        {
            UpdateSimulation(timer);
            timer = 0;
            Renderer.Sync();

            byte[] array = Renderer.BufferGetData(Particle.Position);
            float[] vecArray = new float[array.Length / (int)ByteCount.glfloat];
            Buffer.BlockCopy(array, 0, vecArray, 0, array.Length);
            for (int i = 0; i < vecArray.Length; i += 2)
            {
                Display.Multimesh.SetInstanceTransform2D(i / 2, Transform2D.Identity.Translated(new Vector2(vecArray[i], vecArray[i + 1])));
            }
        }
        //GD.Print("instantiatePositions: " + bufferToString<float>(Particle.InstantiatePosition, ByteCount.glfloat));
        //GD.Print("positions: " + bufferToString<float>(Particle.Position, ByteCount.glfloat));
        //printBuffer<int>(Particle.ToInstantiate, ByteCount.glint);
        //GD.Print();

        if (Input.IsActionJustPressed("F"))
            InstantiateParticles(2, Player.instance.GlobalPosition);
        if (Input.IsActionJustPressed("P"))
        {
            


        }


    }
    public override void _ExitTree()
    {
        base._ExitTree();
        Renderer.FreeRid(Shader);
        Renderer.FreeRid(Pipeline);
        Renderer.FreeRid(Enemy.Position);
        Renderer.FreeRid(Enemy.Radius);
        Renderer.FreeRid(Enemy.Damage);
        Renderer.FreeRid(Particle.Position);
        Renderer.FreeRid(Particle.Velocity);
        Renderer.FreeRid(Particle.InUse);
        Renderer.FreeRid(Particle.ToInstantiate);
        Renderer.FreeRid(Particle.InstantiatePosition);
        Renderer.FreeRid(Particle.Misc);
        Renderer.FreeRid(PlayerInput.FieldPosition);
        Renderer.FreeRid(PlayerInput.FieldVelocity);
        Renderer.FreeRid(PlayerInput.FieldMagnitude);
        Renderer.FreeRid(PlayerInput.PatternIndex);
        Renderer.FreeRid(PlayerInput.FieldSize);
        for (int i = 0; i < 16; i++)
        {
            Renderer.FreeRid(Sampler[i]);
            Renderer.FreeRid(Patterns[i]);
        }
    }
}
