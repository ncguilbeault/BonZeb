#version 400

// Define colour variable which is determined from Bonsai
uniform vec4 colour;

// Define OpenGL incoming and outgoing variables
in vec2 tex_coord;
out vec4 frag_colour;

void main()
{
  frag_colour = colour;
}
