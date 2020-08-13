#version 400
uniform float scale;
in vec2 tex_coord;
out vec4 fragColour;

void main()
{
  fragColour = vec4(1.0*scale, 1.0*scale, 1.0*scale, 1.0);
}
