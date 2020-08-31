#version 400
in vec2 vp;
in vec2 vt;
out vec2 texCoord;

void main()
{
  gl_Position = vec4(vp, 0.0, 1.0);
  texCoord = vt;
}
