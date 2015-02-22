// Ez a pixel shader alak�tja ki a v�gleges k�pet a bluroltb�l �s az eredetib�l

// A Bloomolt k�p text�r�j�t a spritebatch teszi s0-ba
sampler GlowSampler : register(s0);
// mi pedig betett�k s1-be az eredeti�t!
sampler BaseSampler : register(s1);

// Ezekkel v�ltoztatjuk a sz�ntel�tetts�get
// float BaseSaturation;
// float GlowSaturation;

// Ezek a param�terek azt hat�rozz�k meg, hogy melyik k�p mennyire hangs�lyos forr�s
float BaseIntensity;
float GlowIntensity;

// Ez a f�ggv�ny m�dos�tja egy adott sz�n telitetts�g�t
// Megj.: A "sima" saturation intristic f�ggv�ny csup�n annyit csin�l, hogy
// 0 �s 1 k�z� szor�tja az �rt�keket, ez teljesen m�s mint az a szatur�ci�...
//float4 AdjustSaturation(float4 color, float saturation)
//{
	//// Megn�zz�k a sz�n mennyire t�r el a megadott�l(dot product)
	//float grey = dot(color, float3(0.3, 0.59, 0.11));
	//// az eredm�nyt felhaszn�lva line�risan interpol�lunk az ilyen sz�rke �s az 
	//// eredeti sz�n k�z�tt a saturation param�ter alapj�n
	//return lerp(grey, color, saturation);
//}

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	// Kinyerj�k a sz�neket
    float4 glow = tex2D(GlowSampler, texCoord);
	float4 base = tex2D(BaseSampler, texCoord);

	// Szatur�ljuk �s az er�ss�g�ket �ll�tjuk
	//glow = AdjustSaturation(glow, GlowSaturation) * GlowIntensity;
    //base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
	glow = glow * GlowIntensity;
    base = base * BaseIntensity;

	// A nagyon bloomos r�szekn�l s�t�t�tj�k az eredeti k�pet, hogy kicsit
	// megmaradjanak a kont�rok az�rt
	//base *= (1 - saturate(glow));

	return base + glow;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
