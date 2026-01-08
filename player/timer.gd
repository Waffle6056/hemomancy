extends Timer
@export
var target : Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	#print("called ready")
	start();
	timeout.connect(target.queue_free);
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	#print(time_left)
	pass
