extends Node


func _ready() -> void:
	var viewport = $SubViewport
	var texture = viewport.get_texture()
	await RenderingServer.frame_post_draw
	
	var image = texture.get_image()
	var image_texture = ImageTexture.create_from_image(image)
	
	ResourceSaver.save(image_texture,"res://output.tres")
