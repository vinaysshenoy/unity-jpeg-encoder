#pragma strict
#pragma implicit
#pragma downcast

var target:Transform;
var axis:Vector3;
var	speed:float;

function LateUpdate ()
{
	transform.RotateAround(target.position, axis, speed * (Time.deltaTime / Time.timeScale));
}