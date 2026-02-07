shader_type canvas_item;

uniform float white_amount : hint_range(0.0, 1.0) = 0.0;
uniform float alpha_amount : hint_range(0.0, 1.0) = 1.0;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    COLOR = vec4(mix(tex.rgb, vec3(1.0), white_amount), tex.a * alpha_amount);
}