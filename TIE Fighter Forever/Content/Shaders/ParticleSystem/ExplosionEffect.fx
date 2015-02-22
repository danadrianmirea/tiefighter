// Robban�s shader a r�szecskerendszerhez spherical billboard m�dszerrel

float4x4 world;
float4x4 view;
float4x4 projection;
float3 camPos;
float3 camUp;
float3 force;
float time;
float alpha;
float scale;

Texture baseTexture;
sampler textureSampler = sampler_state
{
	texture   = <baseTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

struct VertexShaderInput
{
	// Poz�ci�
	float3 position					: POSITION0;
	// Text�rakoordin�ta(xy), sz�let�si id�(z) �s hal�l ideje(w)
	float4 texAndData				: TEXCOORD0;
	// Elmozdul�svektor(xyz) �s egyedis�g(w)
	float4 deltaMoveAndRand			: TEXCOORD1;
};

struct VertexShaderOutput
{
	float4	position	: POSITION;
	float2	texCoord	: TEXCOORD0;
	float	a			: COLOR0;
};

// A billboard k�z�ppontj�b�l kiteszi a vertexet a billboardnak megfelel� helyre
// a text�rakoordin�t�ja alapj�n, megfelel� m�rettel
float3 BillboardVertex(float3 billboardCenter, float2 cornerID, float size)
{
	// Kisz�moljuk a kamer�t�l a billboard fel� mutat� szemvektort
	float3 eyeVector = billboardCenter - camPos;		
	
	// A billboard jobbra ir�ny� vektora az eyeVectorra �s a kamera f�lfele ir�nyul�
	// vektor�ra is mer�leges, ezt kisz�molhatjuk keresztszorz�ssal.
	// megj.: ez lesz a t�nylegesen jobbra ir�ny, megfelel az xna
	// koordin�tarendszer�nek. Normaliz�ljuk, hogy k�nny� legyen dolgozni vele
	float3 sideVector = cross(eyeVector, camUp);
	sideVector = normalize(sideVector);
	// A billboard felfele vektora a szemvektorra �s az el�bb sz�moltra mer�leges
	// �s az is jobbkezes koordin�tarendszert alkot, teh�t direktszorzattal 
	// sz�molhat�. Ezt is normaliz�ljuk.
	float3 upVector = cross(sideVector,eyeVector);
	upVector = normalize(upVector);
	
	// Az adott vertexet el kell tolni a billboard k�zep�b�l
	float3 finalPosition = billboardCenter;
	// A text�rakoordin�t�k alapj�n
	finalPosition += (cornerID.x-0.5f)*sideVector*size;
	finalPosition += (0.5f-cornerID.y)*upVector*size;	
	
	// Az eredm�ny a vertex kitoltja lesz a megfelel� param�terekkel
	return finalPosition;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	// Kisz�m�tjuk a world m�trixxal transzform�lt r�szecskepoz�ci�t
	float3 startingPosition = mul(float4(input.position, 1), world);

	// Kiszedj�k a param�terez�st a vertexchannel-b�l
	float2 tex = input.texAndData.xy;
	float3 delta = input.deltaMoveAndRand.xyz;
	float random = input.deltaMoveAndRand.w;
	float born = input.texAndData.z;
	float maxAge = input.texAndData.w;

	// kisz�moljuk a vertex r�szecsk�j�nek a jelenlegi �letkor�t [0..1]-ben hol j�r
	// az �lete �ppen.
	float age = time - born;	
	float relAge = age/maxAge;

	// 1-(relAge^2 / 2) alak� f�ggv�ny szerint m�retezz�k a r�szecsk�nket, szorozva
	// a m�retez� shaderkonstanssal �s a v�letlenszer� �rt�kkel ami a r�szecsk�hez tartozik
	float sizer = saturate(1-relAge*relAge/2.0f);
	float size = scale*random*sizer;

	// A szinusz f�ggv�ny els� negyed�t(felszorozva) �s a v�letlent haszn�lva
	// megv�ltoztatjuk a r�szecske sebess�g�t, hogy id�vel(t�volabb) lassuljon le!
	float displacement = sin(relAge*6.28f/4.0f) * 3.0f * random;

	// Kisz�moljuk a billboard eltol�s�t
	float3 billboardCenter = startingPosition + displacement * delta;
	// Ez pedig arra j�, hogy h�zzuk a billboardunkat valamerre(gravit�ci�, vagy 
	// mozg� haj�, vagy fel�letr�l visszapattan�s egyszer�bb emul�l�sa)
	billboardCenter += age * force / 1000;

	// Kisz�moljuk a vertex t�nyleges poz�ci�j�t
	float3 finalPosition = BillboardVertex(billboardCenter, tex, size);
	float4 finalPosition4 = float4(finalPosition, 1);

	float4x4 viewProjection = mul (view, projection);
	output.position = mul(finalPosition4, viewProjection);

	// A PS-nek tudnia kell, hogy mennyire s�t�t a r�szecske
	// (ford�tott n�gyzetf�ggv�nnyel s�t�t�tj�k majd a text�r�ban l�v� sz�nt)
	output.a = 1-relAge*relAge;
	// megj.: az itt kisz�molt a-nak semmi k�ze a majd lent kisz�molt alpha-hoz!!!

	// A text�rakoordin�t�kkal m�r nem csin�lunk semmilyen transzform�ci�t
	output.texCoord = tex;

	// VS ezennel v�gzett
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Kiszedj�k a text�r�t a samplerrel
    float3 rgb = tex2D(textureSampler, input.texCoord);
	// majd a t�vols�g ar�ny�ban megint csak szorozzuk a sz�n�rt�keket
	rgb *= input.a;

	// v�g�l visszaadjuk a k�sz pixelsz�nt a konstans shaderparam�ter Alpha-val
	// kieg�sz�tve!
    return float4(rgb, alpha);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
