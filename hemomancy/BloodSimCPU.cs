using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

public partial class BloodSimCPU : Node
{
    [Export]
    MultiMeshInstance2D Display;
    [Export]
    uint MaxEnemyCount = 1000;
    [Export]
    uint MaxParticleCount = 1000000;
    [Export]
    uint MaxMomentaryInstantiationPositions = 100;
    // first ind is the sum, subsequent indexes are the amount of particles instantiated at the corresponding position minus 1 in InstantiatePosition
    // done this way so its easy to update the buffer ᓚᘏᗢ
    int[] ToInstantiate; 
    Vector2[] InstantiatePosition;
    int InstantiateInd = 0;
    [Export]
    ImageTexture[] PatternData = new ImageTexture[16];
    [Export]
    ImageTexture[] PatternData2 = new ImageTexture[16];
    [Export]
    uint MaxFieldCount = 1000;
    RenderingDevice Renderer = RenderingServer.GetRenderingDevice();

    enum ByteCount
    {
        glvec2 = 8,
        glvec4 = 16,
        glfloat = 4,
        glbool = 4,
        glint = 4,
        gltransform2d = 24,

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
        public Rid FieldTransform;
        public Rid FieldVelocityRot;
        public Rid FieldMagnitude;
        public Rid PatternIndex;
    }
    playerStruct PlayerInput;
    Rid[] Sampler = new Rid[16];
    Rid[] Patterns = new Rid[16];
    Rid[] Sampler2 = new Rid[16];
    Rid[] Patterns2 = new Rid[16];



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
        data.Add(NewUniform(PlayerInput.FieldTransform   = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.gltransform2d])));
        data.Add(NewUniform(PlayerInput.FieldVelocityRot = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glvec4])));
        data.Add(NewUniform(PlayerInput.FieldMagnitude   = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glvec2])));
        data.Add(NewUniform(PlayerInput.PatternIndex     = NewStorageBuffer(new byte[MaxFieldCount * (int)ByteCount.glint])));
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
            Width = (uint)512,//samplePatterns[i].GetWidth(),
            Height = (uint)512,//samplePatterns[i].GetHeight(),
            Depth = 16,
            TextureType = RenderingDevice.TextureType.Type2DArray,
            Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
            UsageBits = RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.SamplingBit,
        };
        //GD.Print(PatternData[0].GetFormat());


        RDUniform patternUniform = new RDUniform
        {
            UniformType = RenderingDevice.UniformType.SamplerWithTexture,
            Binding = bindingInd++
        };
        RDUniform patternUniform2 = new RDUniform
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
            Sampler2[i] = Renderer.SamplerCreate(samplerState);
            Patterns2[i] = Renderer.TextureCreate(format, new RDTextureView());
            patternUniform2.AddId(Sampler2[i]);
            patternUniform2.AddId(Patterns2[i]);
        }

        data.Add(patternUniform);
        data.Add(patternUniform2);
        UpdatePatternData();
        Display.Multimesh.InstanceCount = (int)MaxParticleCount;
        data.Add(NewUniform(RenderingServer.MultimeshGetBufferRdRid(Display.Multimesh.GetRid())));

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
        //Renderer.Submit();
    }

    void ProcessOutputDamageData(byte[] array)
    {
        int[] intArray = new int[array.Length / (int) ByteCount.glint];
        Buffer.BlockCopy(array, 0, intArray, 0, array.Length);
        foreach (int i in HasHP.ActiveIndexes)
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
        while (HasHP.InactiveQueued.Count > 0)
            HasHP.InactiveIndexes.Enqueue(HasHP.InactiveQueued.Dequeue());
        float[] positionData   = new float[MaxEnemyCount * (int) ByteCount.glvec2 / (int) ByteCount.glfloat];
        float[] radiusData     = new float[MaxEnemyCount];
        int[]   outputDamageData = new int[MaxEnemyCount];
        foreach (int i in HasHP.ActiveIndexes)
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
        //GD.Print("called");
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
        while (ManipulationField.InactiveQueued.Count > 0)
            ManipulationField.InactiveIndexes.Enqueue(ManipulationField.InactiveQueued.Dequeue());
        float[]   transformData    = new float[MaxFieldCount * (int) ByteCount.gltransform2d / (int) ByteCount.glfloat];
        float[]   velocityRotData  = new float[MaxFieldCount * (int) ByteCount.glvec4 / (int) ByteCount.glfloat];
        float[]   magnitudeData    = new float[MaxFieldCount * (int) ByteCount.glvec2 / (int) ByteCount.glfloat];
        int[]     patternIndexData = new int[MaxFieldCount];
        foreach (int i in ManipulationField.ActiveIndexes)
        {
            ManipulationField node = ManipulationField.FieldList[i];

            Transform2D test = node.GlobalTransform;

            transformData[i * 6 + 0] =test[0][0]; 
            transformData[i * 6 + 1] =test[0][1];  
            transformData[i * 6 + 2] =test[1][0];
            transformData[i * 6 + 3] =test[1][1];
            transformData[i * 6 + 4] =test[2][0];
            transformData[i * 6 + 5] =test[2][1];
            velocityRotData[i*4+0] = node.Velocity.X;
            velocityRotData[i*4+1] = node.Velocity.Y;

            velocityRotData[i*4+2] = node.RotationSpeed;
            magnitudeData[i*2+0] = node.VelocityMagnitude;
            magnitudeData[i*2+1] = node.AccelerationMagnitude;
            patternIndexData[i] = node.Pattern;
        }
        //GD.Print(String.Join(',', velocityRotData));
        byte[] transformBytes    = ToByteArray(transformData, ByteCount.glfloat);
        byte[] velocityRotBytes  = ToByteArray(velocityRotData, ByteCount.glfloat);
        byte[] magnitudeBytes    = ToByteArray(magnitudeData,ByteCount.glfloat);
        byte[] patternIndexBytes = ToByteArray(patternIndexData, ByteCount.glint);
        Renderer.BufferUpdate(PlayerInput.FieldTransform   , 0, (uint) transformBytes.Length, transformBytes);
        Renderer.BufferUpdate(PlayerInput.FieldVelocityRot , 0, (uint) velocityRotBytes.Length, velocityRotBytes);
        Renderer.BufferUpdate(PlayerInput.FieldMagnitude   , 0, (uint) magnitudeBytes.Length, magnitudeBytes);
        Renderer.BufferUpdate(PlayerInput.PatternIndex     , 0, (uint) patternIndexBytes.Length, patternIndexBytes);
        //TODO patterns
    }
    
    void UpdatePatternData()
    {
        for (int i = 0; i < Patterns.Length; i++)
        {
            if (PatternData[i] != null)
                Renderer.TextureUpdate(Patterns[i], 0, PatternData[i].GetImage().GetData());
            if (PatternData2[i] != null)
                Renderer.TextureUpdate(Patterns2[i], 0, PatternData2[i].GetImage().GetData());
        }
    }

    void UpdateSimulation(double delta)
    {
        UpdateEnemyData(delta);
        UpdateParticleData(delta);
        UpdatePlayerData(delta);
        RenderingServer.CallOnRenderThread(Callable.From(() => { RunCompute(); }));
    }
    public override void _Ready()
	{
		Shader = CompileShader("res://hemomancy/bloodsim.glsl");
        InitializeCompute();

    }
    String bufferToString<E>(Rid buffer, ByteCount type)
    {
        byte[] array = Renderer.BufferGetData(buffer);
        E[] vecArray = new E[array.Length / (int)type];
        Buffer.BlockCopy(array, 0, vecArray, 0, array.Length);
        return String.Join(",", vecArray);

    }
    double timer = 0;
    double interval = 1/24f;
    bool toggle = false;
    double toggleFrequency = 250;
    double toggleHeld = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        //GD.Print();
        if (Input.IsActionJustPressed("1"))
        {
            toggle = !toggle;
        }

        if (Input.IsActionJustPressed("2"))
            InstantiateParticles(1000, Player.instance.GlobalPosition);
        if (Input.IsActionJustPressed("P"))
        {
            GD.Print("particle position data : " +bufferToString<float>(Particle.Position,ByteCount.glfloat));
            //GD.Print("mesh mat data : " +bufferToString<float>((RenderingServer.MultimeshGetBufferRdRid(Display.Multimesh.GetRid())),ByteCount.glfloat));



        }
    }
    public override void _PhysicsProcess(double delta)
    {
        timer += delta;
        if (timer > interval)
        {

            if (toggle)
            {
                toggleHeld += interval * toggleFrequency;
                InstantiateParticles((int)toggleHeld, Player.instance.GlobalPosition);
                toggleHeld -= (int)toggleHeld;
            }
            UpdateSimulation(timer);
            timer = 0;

            //byte[] array = Renderer.BufferGetData(Particle.Position);
            //float[] vecArray = new float[array.Length / (int)ByteCount.glfloat];
            //Buffer.BlockCopy(array, 0, vecArray, 0, array.Length);
            //for (int i = 0; i < vecArray.Length; i += 2)
            //{
            //    Display.Multimesh.SetInstanceTransform2D(i / 2, Transform2D.Identity.Translated(new Vector2(vecArray[i], vecArray[i + 1])));
            //}
            //GD.Print("multimesh: " + bufferToString<float>(RenderingServer.MultimeshGetBufferRdRid(Display.Multimesh.GetRid()), ByteCount.glfloat));

        }
        //GD.Print("instantiatePositions: " + bufferToString<float>(Particle.InstantiatePosition, ByteCount.glfloat));
        //GD.Print("positions: " + bufferToString<float>(Particle.Position, ByteCount.glfloat));
        //printBuffer<int>(Particle.ToInstantiate, ByteCount.glint);

        base._PhysicsProcess(delta);
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
        Renderer.FreeRid(PlayerInput.FieldTransform);
        Renderer.FreeRid(PlayerInput.FieldVelocityRot);
        Renderer.FreeRid(PlayerInput.FieldMagnitude);
        Renderer.FreeRid(PlayerInput.PatternIndex);
        for (int i = 0; i < 16; i++)
        {
            Renderer.FreeRid(Sampler[i]);
            Renderer.FreeRid(Patterns[i]);
        }
    }
}
