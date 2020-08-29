#version 400

uniform float fish_position_x;
uniform float fish_position_y;
uniform float angle;
uniform int switch_stim;
uniform float time;

const float speed = 4.0;
const float frequency = 4.0;

const float twopi = 6.28318530718;
const float halfpi = 1.57079632679;
const float threehalvespi = 4.71238898038;

in vec2 tex_coord;
out vec4 frag_colour;

void gratings() 
{

  float value = 0.5f * sin(((tex_coord.x - (fish_position_y / 1088.0)) * sin(angle + (threehalvespi)) + (tex_coord.y - (fish_position_x / 1088.0)) * cos(angle + (threehalvespi))) * twopi * frequency - speed * phase) + 0.5f;
  frag_colour = vec4(value, value, value, 1);
  return;

}

void gratings_2() 
{

  float value = 0.5f * sin(((tex_coord.x - (fish_position_y / 1088.0)) * sin(angle + (halfpi)) + (tex_coord.y - (fish_position_x / 1088.0)) * cos(angle + (halfpi))) * twopi * frequency - speed * phase) + 0.5f;
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