#version 330 core

in vec2 vTexCoord;

uniform sampler2D uSceneTexture;
uniform float uTime;
uniform vec3 uMirrorTint;
uniform float uShimmerStrength;
uniform float uGlowHeight;

out vec4 FragColor;

void main()
{
    float wave = sin(vTexCoord.y * 42.0 + uTime * 3.5) * uShimmerStrength;
    vec2 warpedUv = vec2(clamp(vTexCoord.x + wave, 0.0, 1.0), vTexCoord.y);

    vec4 baseSample = texture(uSceneTexture, warpedUv);
    vec3 tinted = mix(baseSample.rgb, baseSample.rgb * uMirrorTint, 0.45);

    // Subtle horizontal scanline sheen
    float scanline = 0.96 + 0.04 * sin(vTexCoord.y * 180.0 + uTime * 2.0);
    tinted *= scanline;

    // Soft cyan glow along the mirror line (top edge of the Mirror World FBO)
    float distFromMirrorLine = 1.0 - vTexCoord.y;
    float glow = smoothstep(max(uGlowHeight, 0.001), 0.0, distFromMirrorLine);
    vec3 glowColor = vec3(0.45, 0.92, 0.96) * glow * 0.35;

    FragColor = vec4(tinted + glowColor, baseSample.a);
}
