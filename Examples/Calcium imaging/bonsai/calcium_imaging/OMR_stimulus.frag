#version 400
in vec2 texCoord;
out vec4 fragColor;
//const float pi = 3.14159265359;
//const float halfpi = 3.14159265359 / 2;
const float twoPi = 2 * 3.14159265359;
uniform float phase;
//uniform float x_pos;
//const float x_pos = 0.5;
//const float speed = 0.5;
uniform float speed;
//const float frequency = 20;
uniform float frequency;
uniform int switch_stim;

void main()
{
  float value;
  switch(switch_stim)
  {
    case 0:
      fragColor = vec4(0.0, 0.0, 0.0, 1.0);
      return;
    case 1:
      value = 0.5 * sin(texCoord.x * frequency * twoPi) + 0.5;
      fragColor = value > 0.5 ? vec4(1.0, 1.0, 1.0, 1.0) : vec4(0.0, 0.0, 0.0, 1.0);
      return;
    case 2:
      value = 0.5 * sin(texCoord.x * frequency * twoPi - (phase * speed)) + 0.5;
      fragColor = value > 0.5 ? vec4(1.0, 1.0, 1.0, 1.0) : vec4(0.0, 0.0, 0.0, 1.0);
      return;
  }
}
