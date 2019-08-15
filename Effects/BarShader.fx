sampler TextureSampler : register(s0);

float4 main(float2 coords : TEXCOORD0) : SV_TARGET
{
	float4 Color = tex2D(TextureSampler, coords);
	return Color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 main();
	}
}