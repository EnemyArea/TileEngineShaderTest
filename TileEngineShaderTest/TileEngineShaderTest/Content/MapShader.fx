float4x4 World;
float4x4 View;
float4x4 Projection;

Texture2D TilesetTexture;
sampler2D tilesetTextureSampler = sampler_state
{
	Texture = <TilesetTexture>;
	Filter = Point;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = WRAP;
	AddressV = WRAP;
};

Texture2D MapTexture;
sampler2D mapTextureSampler = sampler_state
{
	Texture = <MapTexture>;
	Filter = Point;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexShaderInput
{
	float2 TexCoord : TEXCOORD0;
	float4 Position : SV_Position0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float2 TexCoord : TEXCOORD0;
	float4 Position : SV_Position0;
	float4 Color : COLOR0;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;

	return output;
}

// Tilemap Dimension (in tiles)
int MapWidthInTiles;
int MapHeightInTiles;
float4 ColorKey;

// Max Tileset-Tiles per row
float TilesetSizeInTiles = 64.0;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 pixel = tex2D(mapTextureSampler, input.TexCoord);
	if (pixel.a == 0) return float4(0,0,0,0);

	float2 offset;

	float testPixel = pixel.b * 255;

	if (testPixel == 200) {
		return float4(1, 1, 1, 1);
	}
	else if (testPixel == 0) {
		offset = float2(0, 0);
	}
	else if (testPixel == 1) {
		offset = float2(0.5, 0);
	}
	else if (testPixel == 2) {
		offset = float2(0, 0.5);
	}
	else if (testPixel == 4) {
		offset = float2(0.5, 0.5);
	}
	else if (testPixel > 100) {
		//return float4(1, 1, 1, 1);

		float pixelGroup = testPixel - 100.0;
		float pixelGroupX = (float)trunc(pixelGroup / 10.0);
		float pixelGroupY = (float)fmod(max(pixelGroup - 10.0, pixelGroup), 10.0);

		offset = float2(pixelGroupX * 16.0 / 32.0, pixelGroupY * 16.0 / 32.0);
	
		//return float4(offset.x / 255, offset.y / 255, 0, 1);
		//return float4(pixelGroupX / 255.0, pixelGroupY / 255.0, 0, 1);
	}

	int colX = (int)ceil((pixel.r * 255.0));
	int colY = (int)ceil(((pixel.g * 255.0) * TilesetSizeInTiles));

	int index = colX + colY;
	int xpos = index % TilesetSizeInTiles;
	int ypos = index / TilesetSizeInTiles;

	float xoffset = frac(input.TexCoord.x * MapWidthInTiles * 2);
	float yoffset = frac(input.TexCoord.y * MapHeightInTiles * 2);
	float2 uv = ((float2(xoffset, yoffset) / 2 + offset) / TilesetSizeInTiles) +
		((float2(xpos, ypos)) / TilesetSizeInTiles);

	return tex2D(tilesetTextureSampler, uv);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
	}
}

//float4x4 World;
//float4x4 View;
//float4x4 Projection;
//
//Texture2D TilesetTexture;
//sampler2D tilesetTextureSampler = sampler_state
//{
//	Texture = <TilesetTexture>;
//	Filter = Point;
//	MinFilter = Point;
//	MagFilter = Point;
//	MipFilter = Point;
//	AddressU = WRAP;
//	AddressV = WRAP;
//};
//
//Texture2D MapTexture;
//sampler2D mapTextureSampler = sampler_state
//{
//	Texture = <MapTexture>;
//	Filter = Point;
//	MinFilter = Point;
//	MagFilter = Point;
//	MipFilter = Point;
//	AddressU = WRAP;
//	AddressV = WRAP;
//};
//
//struct VertexShaderInput
//{
//	float2 TexCoord : TEXCOORD0;
//	float4 Position : SV_Position0;
//	float4 Color : COLOR0;
//};
//
//struct VertexShaderOutput
//{
//	float2 TexCoord : TEXCOORD0;
//	float4 Position : SV_Position0;
//	float4 Color : COLOR0;
//};
//
//struct PixelShaderOutput
//{
//	float4 Color : COLOR0;
//};
//
//VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
//{
//	VertexShaderOutput output;
//	float4 worldPosition = mul(input.Position, World);
//	float4 viewPosition = mul(worldPosition, View);
//
//	output.Position = mul(viewPosition, Projection);
//	output.TexCoord = input.TexCoord;
//	output.Color = input.Color;
//
//	return output;
//}
//
//// Tilemap Dimension (in tiles)
//int MapWidthInTiles;
//int MapHeightInTiles;
//float4 ColorKey;
//
//// Max Tileset-Tiles per row
//float TilesetSizeInTiles = 64.0;
//
//float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
//{
//	float4 pixel = tex2D(mapTextureSampler, input.TexCoord);
//	if (pixel.r == ColorKey.r && pixel.g == ColorKey.g && pixel.b == ColorKey.b) return float4(0,0,0,0);
//
//	int colX = (int)ceil((pixel.r * 255.0));
//	int colY = (int)ceil(((pixel.g * 255.0) * TilesetSizeInTiles));
//
//	float2 offset = float2(pixel.b * 255.0 / 32, pixel.a * 255.0 / 32);
//
//	int index = colX + colY;
//	int xpos = index % TilesetSizeInTiles;
//	int ypos = index / TilesetSizeInTiles;
//
//	float xoffset = frac(input.TexCoord.x * MapWidthInTiles * 2);
//	float yoffset = frac(input.TexCoord.y * MapHeightInTiles * 2);
//	float2 uv = ((float2(xoffset, yoffset) / 2 + offset) / TilesetSizeInTiles) +
//		((float2(xpos, ypos)) / TilesetSizeInTiles);
//
//	return tex2D(tilesetTextureSampler, uv);
//}
//
//technique Technique1
//{
//	pass Pass1
//	{
//		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
//		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
//	}
//}