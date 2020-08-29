#version 400

uniform float fish_position_x;
uniform float fish_position_y;
uniform float angle;
uniform int switch_stim;
uniform float time;

const float dot_dist = 0.1;
const float dot_speed = 1;
const float dot_size = 0.003;

const float thirdpi = 1.0471975512;

in vec2 tex_coord;
out vec4 frag_colour;

void moving_dot() 
{

  vec2 circle_position = vec2(((fish_position_y / 1088.0) + dot_dist * sin(angle + (thirdpi * (sin(phase * dot_speed))))), ((fish_position_x / 1088.0) + dot_dist * cos(angle + (thirdpi * (sin(phase * dot_speed))))));
  if (sqrt(pow(((tex_coord.x - circle_position[0])), 2) + pow((tex_coord.y - circle_position[1]), 2)) < dot_size) 
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  } 
  else 
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;

}

void solid_black_stimulus() 
{

  frag_colour = vec4(0.2, 0.2, 0.2, 1.0);

}

void main() {

  switch (switch_stim) 
  {
    case 0:
      solid_black_stimulus();
      break;
    case 1:
      moving_dot();
      break;
  }

}