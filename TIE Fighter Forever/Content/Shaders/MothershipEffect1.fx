// Egy kis normal mapping

float4x4 World;
float4x4 View;
float4x4 Projection;

// F�ny tulajdons�gai
float3 LightPosition;
float4 LightColor;
float4 AmbientLightColor;

// Anyagtulajdons�gok
// Phong specular-hoz sk�l�z� t�nyez�
float Shininess;
// Exponens a phong lightinghez, a specular csillog�s sz�less�g�t hat�rozza meg
float SpecularPower;

// A kamera poz�ci�ja(world space-ben)
float3 eyePosition;

// Normal map �s a hozz� tartoz� sampler
texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};
// Color map �s a hozz� tartoz� sampler
texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

// Ez a vertex shader inputja, itt pont az van, ami a vertex csatorn�kb�l megmarad!
struct VertexShaderInput
{
    float4 position		: POSITION0;
    float2 texCoord		: TEXCOORD0;
    float3 normal		: NORMAL0;    
    float3 binormal		: BINORMAL0;
    float3 tangent		: TANGENT0;

};

// A Vertex shader kimenete �s a Pixel shader bemenete.
struct VertexShaderOutput
{
    float4 position			: POSITION0;
    float2 texCoord			: TEXCOORD0;
    float3 lightDirection	: TEXCOORD1;
    float3 viewDirection	: TEXCOORD2;	// ut�bbi k�t vektor tangens t�rben!
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// A poz�ci�t transzform�ljuk a world m�trixal
    float4 worldPosition = mul(input.position, World);
    float4 viewPosition = mul(worldPosition, View);
	output.position = mul(viewPosition, Projection);

	// F�ny ir�ny�nak a kisz�m�t�sa("v�g-kezdet")
	output.lightDirection = LightPosition - worldPosition;

	// Ezut�n kisz�moljuk a szemt�l a vertex fel� mutat� vektort
	output.viewDirection = worldPosition - eyePosition;

	// El��ll�tjuk a 3x3-as m�trixot, mely a tangens t�rb�l a vil�gt�rbe helyez minket
	// B�zisnak a mesh tangens, binorm�l �s norm�lvektor�nak world-space-ben vett
	// koordin�t�it vessz�k, ezzel szok�sos ortogon�lis b�zist kapunk, mely k�pes
	// a megfelel� transzform�ci�ra...

	float3x3 tangentToWorld;
	tangentToWorld[0] = mul(input.tangent,    World);
    tangentToWorld[1] = mul(input.binormal,   World);
    tangentToWorld[2] = mul(input.normal,     World);

	// A f�nyt �s a view-t tessz�k tangens t�rbe, nem a PS-ben m�trixszorzunk!
	output.lightDirection = mul(tangentToWorld, output.lightDirection);
	output.viewDirection = mul(tangentToWorld, output.viewDirection);
	// Megj(magamnak).: Mint r�gi k�dban a shadertut k�nyvt�ramban

	// A text�rakoordin�t�kat nem b�ntjuk!
	output.texCoord = input.texCoord;

	// A sz�m�tott �rt�kek a pixel shadernek m�r interpol�lt form�ban
	// �rkeznek meg minden pixelre!
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Ezek csak akkor kellenek, ha van l�pt�kel�s a m�trixokban
	input.viewDirection = normalize(input.viewDirection);
    input.lightDirection = normalize(input.lightDirection);

	// A norm�lvektort kiolvassuk a normal mapb�l
	// A pontoss�g jav�t�sa �rdek�ben a samplel�s ut�n is normaliz�lunk!
	//float3 normalFromMap = normalize(tex2D(NormalMapSampler, input.texCoord));	// Elvileg ez nem t�lzottan kell a preprocess miatt(csak lehetnek hib�k, meg ugye a sample-l�s)!!!
	float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord);

	// A kapott norm�lvektorral diff�z-jelleg� phong shadingot csin�lunk
	float nDotL = max(dot(normalFromMap, input.lightDirection), 0);
    float4 diffuse = LightColor * nDotL;

	// specular phong shading: Visszat�kr�zz�k a f�nyt a norm�lvektorral,
	// majd skal�ris szorz�ssal "vizsg�ljuk" a view vektort�l mennyire t�r el...
	float3 reflectedLight = reflect(input.lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, input.viewDirection), 0.0001f);
	float power = pow(rDotV, SpecularPower);
	float4 specular = Shininess * LightColor * power;

	// A sima diff�z text�r�t is kiolvassuk
	float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);

	// �s visszaadjuk a sz�molt eredm�nysz�nt
	// tesztel�s: return diffuseTexture;
	return (diffuse + AmbientLightColor) * diffuseTexture + specular;
	//return float4(normalFromMap,1);	// tesztel�s
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}