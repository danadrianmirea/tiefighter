// Ez a pixel shader felel�s a gaussian blur-�rt egy egyenes ment�n

// A spritebatch az s0 regiszterbe teszi a text�r�t amit haszn�lunk vele
sampler TextureSampler : register(s0);

// 15 darab sample lesz
#define SAMPLE_COUNT 15
// Ezek eltol�si koordin�t�i �s s�lyai pedig itt tal�lhat�ak:
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;

	// c-be az �tlagolt sz�n ker�l(A s�lyok �sszege egy! Ezt el�re kisz�moltuk!)
	for(int i = 0; i < SAMPLE_COUNT; ++i)
	{
		c += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
	}

	// A blurolt sz�n az eredm�ny�nk
    return c;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
