sampler2D baseMap;

struct PS_INPUT
{
	float2 Texcoord : TEXCOORD0;
};

float4 ps_main(PS_INPUT Input) : COLOR0
{
	float4 color = tex2D(baseMap, Input.Texcoord);
	return float4(color.a, color.a, color.a, color.a);
}

technique Technique1
{
	pass Tint
	{
		PixelShader = compile ps_2_0 ps_main();
	}
}