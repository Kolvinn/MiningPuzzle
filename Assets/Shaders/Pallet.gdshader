shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform vec4[46] colorpallet;

vec4 GetNearest(vec4 screenCol)
{
	if(colorpallet.length() ==0)
	{
		return screenCol;
	}
	vec4 nearest_color;
	float dist = 500.0;
	for(int i =0; i < colorpallet.length();i++){
		vec4 col = colorpallet[i];
		float r = pow(col.r - screenCol.r, 2.0);
		float g = pow(col.g - screenCol.g, 2.0);
		float b = pow(col.b - screenCol.b, 2.0);
		//float a = pow(col.a - screenCol.a, 2.0);
		
		float result = sqrt( r + b + g); //+ a;
		
		
		if(result == 0.0){
			nearest_color = col;
			break;
		}
		else if(result < dist)
		{
			dist = result;
			nearest_color = col;
		}
	}
	return  nearest_color;
}

void fragment() {
	vec4 c = textureLod(screen_texture, SCREEN_UV, 0.0).rgba;
	if(c.a !=0.0)
	{	{}
    
	vec4 thing = GetNearest(c.rgba);
	
	
	
    thing.rgb = mix(vec3(0.0), thing.rgb, brightness);
    thing.rgb = mix(vec3(0.5), thing.rgb, contrast);
    thing.rgb = mix(vec3(dot(vec3(1.0), thing.rgb) * 0.33333), thing.rgb, saturation);
	
	COLOR.rgba = thing;
	}
    //COLOR.rgb = c;
	
	
	//COLOR = GetNearest(COLOR.rgba);
}


//foreach (object o in WebColors)
//{
//	// compute the Euclidean distance between the two colors
//	// note, that the alpha-component is not used in this example
//	vdbl_test_red = pow( - dbl_input_red, 2.0);
//	dbl_test_green = Math.Pow(Convert.ToDouble
//		(((Color)o).G) - dbl_input_green, 2.0);
//	dbl_test_blue = Math.Pow(Convert.ToDouble
//		(((Color)o).B) - dbl_input_blue, 2.0);
//	// it is not necessary to compute the square root
//	// it should be sufficient to use:
//	// temp = dbl_test_blue + dbl_test_green + dbl_test_red;
//	// if you plan to do so, the distance should be initialized by 250000.0
//	temp = Math.Sqrt(dbl_test_blue + dbl_test_green + dbl_test_red);
//	// explore the result and store the nearest color
//	if(temp == 0.0)
//	{
//		// the lowest possible distance is - of course - zero
//		// so I can break the loop (thanks to Willie Deutschmann)
//		// here I could return the input_color itself
//		// but in this example I am using a list with named colors
//		// and I want to return the Name-property too
//		nearest_color = (Color)o;
//		break;
//	}
//	else if (temp < distance)
//	{
//		distance = temp;
//		nearest_color = (Color)o;
//	}
//}
//}
