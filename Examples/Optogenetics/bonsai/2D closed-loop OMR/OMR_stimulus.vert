#version 400

const float xRange = 1.0;
const float yRange = 1.0;
const float xOffset = 0.0;
const float yOffset = 0.0;

in vec2 vp;
in vec2 vt;
out vec2 texCoord;

void main()
{
  gl_Position = vec4((vp.x * xRange) + xOffset, (vp.y * yRange) + yOffset, 0.0, 1.0);
  texCoord = vt;
}
