shader_type canvas_item;

varying vec2 vert;
uniform float speed;

void vertex()
{
	vert = VERTEX.xy;
}

void fragment () {
	COLOR = texture(TEXTURE, vec2(UV.x + TIME * speed, UV.y));
	ivec2 size = textureSize(TEXTURE,0);
	float dist = TEXTURE_PIXEL_SIZE.x * vert.x;//(float(size.x)/vert.x);
	if(COLOR.a != 0.0){
		if( dist > 0.5)
		{
			//dist = dist * 10.0;
			float thing = (1.0 - dist) * 10.0; // 1,2,3,4,5
			//if(thing >1.0){
			COLOR.a = COLOR.a  * thing;
			//}
			
		}
		else
		{
			float thing = (0.5-dist) * 10.0; // 1,2,3,4,5
			float ab = abs(dist - 0.5);
			//if(thing >1.0){
			COLOR.a = COLOR.a  * dist *10.0;
			//COLOR.a = COLOR.a  *  1.0 - (0.5 - dist)*1.2;
		}
	}
	
	
}