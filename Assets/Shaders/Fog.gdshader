shader_type canvas_item;
render_mode unshaded;

uniform sampler2D dissolve_texture;
uniform float dissolve_value : hint_range(0,1);
uniform bool dissolve_bool = false;

uniform bool Smooth = true;
uniform float width : hint_range(0.0, 64) = 1.0;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform int pixel_size : hint_range(1, 10) = 4;
uniform float width_speed :hint_range(0.1, 10) = 1;
// Amount of detail.
uniform int octaves = 4;

// Opacity of the output fog.
uniform float starting_amplitude: hint_range(0.0, 0.5) = 0.5;

// Rate of pattern within the fog.
uniform float starting_frequency = 1.0;

// Shift towards transparency (clamped) for sparser fog.
uniform float shift: hint_range(-1.0, 0.0) = -0.2;

// Direction and speed of travel.
uniform vec2 velocity = vec2(1.0, 1.0);

//uniform sampler2D noise: filter_nearest, repeat_enable;
// Color of the fog.
uniform vec4 fog_color: source_color =  vec4(0.0, 0.0, 0.0, 1.0);

// Noise texture; OpenSimplexNoise is great, but any filtered texture is fine.
uniform sampler2D noise: filter_nearest, repeat_enable;

float rand(vec2 uv) {
	float amplitude = starting_amplitude;
	float frequency = starting_frequency;
	float output = 0.0;
	for (int i = 0; i < octaves; i++) {
		output += texture(noise, uv * frequency).x * amplitude;
		amplitude /= 2.0;
		frequency *= 2.0;
	}
	return clamp(output + shift, 0.0, 1.0);
}

void fragment() {
	float a = texture(TEXTURE, UV).a;
	vec2 motion = vec2(rand(UV + TIME * starting_frequency * velocity));
	COLOR = mix(vec4(0.0), fog_color, rand(UV + motion));
	
	float _width = width;// + ((sin(TIME*width_speed) + 1.0) -2.0) * 10.0;
	vec4 tex = texture(noise, SCREEN_UV);
	vec2 pixel_size2 = 1.0 / vec2(textureSize(TEXTURE, 0));
	vec2 unit = (1.0/float(pixel_size) ) / vec2(textureSize(TEXTURE, 0));
    vec4 pixel_color = COLOR;
	
	if(pixel_color.a == 0.0)
	{
		//COLOR = vec4(1,1,1,1);
	
//	float _width = width + ((sin(TIME*width_speed) + 1.0) -2.0) * 10.0;
//	vec4 tex = texture(noise, UV);
//	//vec4 = TEXTURE;
//    vec2 unit = (1.0/float(pixel_size) ) / vec2(textureSize(noise, 0));
//    vec4 pixel_color = texture(noise, UV);
//    if (pixel_color.a == 0.0) {
        pixel_color = outline_color;
        pixel_color.a = 0.0;
        for (float x = -ceil(_width); x <= ceil(_width); x++) {
            for (float y = -ceil(_width); y <= ceil(_width); y++) {
                if (texture(noise, UV + vec2(x*unit.x, y*unit.y)).a == 0.0 || (x==0.0 && y==0.0)) {
                    continue;
                }
                if (Smooth) 
				{
                    pixel_color.a += outline_color.a / (pow(x,2)+pow(y,2)) * (1.0-pow(2.0, -_width));
                    if (pixel_color.a > 1.0) {
                        pixel_color.a = 1.0;
                   }
                } else 
				{
                    pixel_color.a = outline_color.a;
                }
            }
        }
		COLOR = pixel_color;
		
    }
	COLOR.a = a;
	
	if(dissolve_bool)
	{
		//vec4 main_texture = texture(TEXTURE, UV);
    	vec4 noise_texture = texture(dissolve_texture, UV);
    	COLOR.a *= floor(dissolve_value + min(1, noise_texture.x));
    	//COLOR = main_texture;
	}
    
}
