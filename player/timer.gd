extends Timer
@export
var target : Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	#print("called ready")
	start();
	if (target != null):
		timeout.connect(target.queue_free);
	else:
		timeout.connect(get_parent().queue_free);
	pass # Replace with function body.
