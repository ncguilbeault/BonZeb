#version 400

uniform int switch_stim;
uniform float phase;

const float twoPi = 6.28318530718;
const float speed = 10;
const float frequency = 20;

in vec2 texCoord;
out vec4 fragColor;

void main()
{
  float value;
  switch(switch_stim)
  {
    case 0:
      fragColor = vec4(0.0, 0.0, 0.0, 1.0);
      return;
    case 1:
      value = 0.5 * sin(texCoord.x * frequency * twoPi - (phase * speed)) + 0.5;
      fragColor = value > 0.5 ? vec4(1.0, 1.0, 1.0, 1.0) : vec4(0.0, 0.0, 0.0, 1.0);
      return;
  }
}
