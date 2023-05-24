#ifdef GL_ES
precision mediump float;
#endif

uniform vec2 u_resolution;
uniform float u_time;

float random(vec2 st){
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void main(){
    vec2 st = gl_FragCoord.xy / u_resolution.xy;
    st.x += mod(u_time, 8.0);

    float ship_hp = abs(sin(u_time / 5.0));
    float dice = step(0.5, random(floor(st * 100.0)));
    float colorParam = ship_hp * 1.5 * dice;

    gl_FragColor = vec4(1.0, colorParam, colorParam, gl_FragCoord.xy / u_resolution.xy);
}