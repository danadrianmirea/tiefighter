// Environment mapping effect

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection;
float4 LightColor;
float3 AmbientColor;

float Glow;

// A kamera poz�ci�ja(world space-ben)
float3 eyePosition;

bool TextureEnabled;

texture Texture;
texture EnvironmentMap;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Reflection : TEXCOORD1;
    float3 Fresnel : COLOR0;
    float3 Lighting : COLOR1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// A poz�ci�t transzform�ljuk a world m�trixal
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	// A text�rakoordin�t�kat nem b�ntjuk
	output.TexCoord = input.TexCoord;

	// Kisz�m�tjuk a vertex worldspace-beli norm�lvektor�t
	// Megj.: Feltessz�k, hogy nincs l�pt�kez�s(scale), k�l�nben
	// float3 worldNormal = normalize(mul(input.Normal, World));
	// kellene ahelyett, hogy:
	float3 worldNormal = mul(input.Normal, World);

	// A szem koordin�t�j�t sz�moljuk ki a vil�gt�rben. Ezt �gy tessz�k, 
	// hogy vessz�k a View transzform�ci� eltol�s r�sz�t �smegszorozzuk
	// a View m�trix balfels� 3x3as�nak(forgat�si r�sz) inverz�vel
	// Mivel a m�trix ortonorm�lis, az inverz itt transzpon�l�st jelent.
	// Megj.: Ezt ink�bb CPU-val sz�moljuk ki �s nem minden vertexre!!!
    //float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));
	// Ezut�n kisz�moljuk a szemt�l a vertex fel� mutat� vektort
	float3 viewVector = worldPosition - eyePosition;
	// Egy intristic f�ggv�nnyel kisz�moljuk a el�z�leg sz�molt vektor
	// a vil�gt�rbeli norm�lvektorra t�kr�z�ttj�t
	output.Reflection = reflect(viewVector, worldNormal);
	// A fresnel egy�tthat�t k�zel�tj�k azzal, hogy skal�rszorozzuk
	// a n�zeti �s a nom�lvektort, majd [0..1]-beliv� tessz�k az eredm�nyt
    output.Fresnel = saturate(1 + dot(normalize(viewVector), worldNormal));
	// A f�ny sz�m�t�sa is hasonl�an zajlik
    float lightAmount = max(dot(worldNormal, normalize(LightDirection)), 0);
	output.Lighting = AmbientColor + lightAmount * LightColor;

	// A sz�m�tott �rt�kek a pixel shadernek m�r interpol�lt form�ban
	// �rkeznek meg minden pixelre!
    return output;
}

// Sampler a "sima" text�r�k el�r�s�hez
sampler TextureSampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Wrap;
    AddressV = Wrap;
};
// Sampler az environment text�r�k el�r�s�hez
sampler EnvironmentMapSampler = sampler_state
{
    Texture = (EnvironmentMap);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 color;
	// Lehet, hogy nincs a modelhez diff�z text�ra, ekkor legyen fekete
	// egy�bk�nt a tex2D-vel olvassunk a sima samplerb�l.
	if (TextureEnabled)
        color = tex2D(TextureSampler, input.TexCoord);
    else
        color = float3(0, 0, 0);
	// Az envmap sz�n�rt�k�t a texCUBE f�ggv�nnyel olvassuk
	// a texcube fv. val�s�tja meg a cubemapokb�l t�rt�n� helyes olvas�st!
	float3 envmap = texCUBE(EnvironmentMapSampler, -input.Reflection);
	// Az interpol�lt �s k�zel�t� fresnel �rt�kkel v�gezz�nk line�ris
	// interpol�ci�t(lerp) a sz�n �s az environment sz�n k�z�tt!
    color = lerp(color, envmap, input.Fresnel);

    // A sz�neket ut�kezelj�k a f�ny hat�s�ra
    color *= input.Lighting * 2;

	// alpha csatorna 1-re �r�s�val t�r�nk vissza
	return float4(color, Glow);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
