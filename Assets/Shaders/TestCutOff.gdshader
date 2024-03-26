shader_type spatial;
uniform sampler2D _albedo :source_color;
uniform mat4 cutplane;
varying vec3 worldSpace;

void vertex(){
	worldSpace = (MODEL_MATRIX * vec4(VERTEX,1.0)).xyz;
}
void fragment() {
	ALPHA = 0.0f;
	//ALBEDO = 0.0;
	//ALBEDO = texture(COLOR, UV).rbg;
	vec3 planeNormal = normalize(-cutplane[1].xyz);
	float planeDistance = dot(planeNormal, cutplane[3].xyz);
	float vertexDistance = dot(planeNormal, worldSpace);
	float dist = vertexDistance -planeDistance; 
	
	//if(dist < 0.0)
		//discard;
	
}
