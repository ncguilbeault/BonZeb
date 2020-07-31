#version 400

// Define OpenGL incoming and outgoing variables
in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4(vp, 0.0, 1.0);
  tex_coord = vt;
}
