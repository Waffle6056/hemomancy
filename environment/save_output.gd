extends Node
@export
var frames : int

func _ready() -> void:
	var viewport = $SubViewport
	var texture = viewport.get_texture()
	
	for c in viewport.get_children():
		if (!c.is_in_group("pattern_renderable")):
			continue
		c.visible=true;
		for i in frames:
			c.material.set_shader_parameter('frame_active', i+1);
			print(str(i+1)+c.name);
			await RenderingServer.frame_post_draw
		
			var image = texture.get_image()
			#print(image.get_format())
			var image_texture = ImageTexture.create_from_image(image)
		
			ResourceSaver.save(image_texture,"res://hemomancy/patterns/"+c.name+"/"+c.name+str(i+1)+".tres")
		
		c.visible=false;
