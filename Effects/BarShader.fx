// Vertex Shader input structure consisting of a position and color
struct VertexShaderInput
{
	float4 Position : SV_Position;
	float4 Color : COLOR;
};

// Vertex Shader output structure consisting of the
// transformed position and original color
struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR;
	float4 Depth: TEXCOORD0;
};

// Vertex shader main function
VertexShaderOutput VSMain(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Transform the position from object space to homogeneous projection space
	output.Position = input.Position;
	// Pass through the color of the vertex
	output.Color = input.Color;

	// Put the position into Depth as well as the 
	// output.Position gets modified before it gets to the
	// the pixel shader.
	output.Depth = output.Position;

	return output;
}

// A simple Pixel Shader that simply passes through the interpolated color
float4 PSMain(VertexShaderOutput input) : SV_Target
{
	float4 output = (float4)input.Depth.z / input.Depth.w;
	output.w = 1.0;
	return input.Color;
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 VSMain();
		PixelShader = compile ps_2_0 PSMain();
    }
}