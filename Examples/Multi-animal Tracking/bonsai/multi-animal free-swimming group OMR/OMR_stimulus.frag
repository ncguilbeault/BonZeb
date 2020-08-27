#version 400

uniform int switch_stim;
uniform float phase;

const float speed = 12.0;
const float frequency = 6.0;

const float twopi = 6.28318530718;
const float pi = 3.14159265359;

in vec2 tex_coord;
out vec4 frag_colour;

void gratings() 
{

  float value = 0.5f * sin(tex_coord.x * twopi * frequency - speed * phase) + 0.5f;
  frag_colour = vec4(value, value, value, 1);
  return;

}

void gratings_2() 
{

  float value = 0.5f * sin(tex_coord.y * twopi * frequency - speed * phase) + 0.5f;
  frag_colour = vec4(value, value, value, 1);
  return;

}

void solid_black_stimulus() 
{

  frag_colour = vec4(0.0, 0.0, 0.0, 1.0);

}

void main() 
{

  switch (switch_stim) 
  {
    case 0:
      solid_black_stimulus();
      break;
    case 1:
      gratings();
      break;
    case 2:
      gratings_2();
      break;
  }

}
