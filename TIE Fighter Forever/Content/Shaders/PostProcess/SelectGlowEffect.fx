// Ez a pixel shader v�gzi a glowol� r�szek kiv�logat�s�t!

// A spritebatch az s0 regiszterbe teszi a text�r�t amit haszn�lunk vele
sampler TextureSampler : register(s0);

float GlowBonus;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // kiszedj�k a pixel sz�n�t(RGBA)
	float4 c = tex2D(TextureSampler, texCoord);

	c.a -= GlowBonus;

	// Min�l kissebb az alfa �rt�k, ann�l er�sebb lesz a sz�n�rt�k
	float selector = (1.0 - c.a);

	return c * selector;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
