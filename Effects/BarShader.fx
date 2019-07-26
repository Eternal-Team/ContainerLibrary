float4 main(float2 coords : TEXCOORD0) : SV_TARGET
{
	return 1;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 main();
	}
}