#version 400

const float xRange = 0.2979166806;
const float yRange = 0.5296296477;
const float xOffset = -0.1000000015;
const float yOffset = -0.057407409;

in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4((vp.x * xRange) + xOffset, (vp.y * yRange) + yOffset, 0.0, 1.0);
  tex_coord = vt;
}
