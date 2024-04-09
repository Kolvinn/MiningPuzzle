shader_type canvas_item;
uniform bool filled; 
uniform float alpha;
void fragment() 
{

	
	if(filled && COLOR.r > 0.8)
	{
		COLOR.a = alpha;
	}
	COLOR.rgb = COLOR.rgb * 1.15;
	
}
