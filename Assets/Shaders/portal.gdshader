shader_type canvas_item;
uniform float amount = 0.1;
void fragment() {
	vec4 thing = texture(TEXTURE, UV).rgba;
	
	if(thing.r < amount && thing.b < amount && thing.g < amount){
		thing.a = 0.0;
	}
	COLOR = thing;
}
