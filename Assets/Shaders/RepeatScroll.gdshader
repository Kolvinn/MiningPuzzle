shader_type canvas_item;

varying vec2 vert;
uniform float speed;

void vertex()
{
	vert = VERTEX.xy;
}

vec4 GetScroll(vec4 col, ivec2 size, float dist){
	
	vec4 color = col;
	if(color.a != 0.0)
	{
		if( dist > 0.5)
		{
			//dist = dist * 10.0;
			float thing = (1.0 - dist) * 10.0; // 1,2,3,4,5
			//if(thing >1.0){
			color.a = color.a  * thing;
			//}
			
		}
		else
		{
			float thing = (0.5-dist) * 10.0; // 1,2,3,4,5
			float ab = abs(dist - 0.5);
			//if(thing >1.0){
			color.a = color.a  * dist *10.0;
			//COLOR.a = COLOR.a  *  1.0 - (0.5 - dist)*1.2;
		}
	}
	return color;
}
void fragment () 
{
	
	vec4 color = texture(TEXTURE, vec2(UV.x + TIME * speed, UV.y));
	ivec2 size = textureSize(TEXTURE,0);
	float dist = TEXTURE_PIXEL_SIZE.x * vert.x;//(float(size.x)/vert.x);
	COLOR = GetScroll(color, size, dist);
}