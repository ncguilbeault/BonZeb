#version 400

const float xRange = 0.153125003;
const float yRange = 0.2509259284;
const float xOffset = -0.196875006;
const float yOffset = -0.0916666687;

in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4((vp.x * xRange) + xOffset, (vp.y * yRange) + yOffset, 0.0, 1.0);
  tex_coord = vt;
}
