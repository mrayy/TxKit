
float2 LensCenter=float2(0.5,0.5);
float2 FocalLength=float2(1,1);
float4 WrapParams=float4(1,1,1,1);
//float4 TextureRect=float4(0,0,1,1);//x,y,width,height

float2 _CorrectDistortion(float2 uv)
{ 
	//uv.x=(uv.x-TextureRect.x)/(float)(TextureRect.z);
	//uv.y=(uv.y-TextureRect.y)/(float)(TextureRect.w);
	float2 xy=(uv-LensCenter)/FocalLength;

	float r=sqrt(dot(xy,xy));
	float r2=r*r;
	float r4=r2*r2;
	float coeff=(WrapParams.x*r2+WrapParams.y*r4); //radial factor

	float dx=WrapParams.z*2.0*xy.x*xy.y    + WrapParams.w*(r2+2.0*xy.x*xy.x);
	float dy=WrapParams.z*(r2+2.0*xy.y*xy.y) + WrapParams.w*2.0*xy.x*xy.y;

	xy=((xy+xy*coeff.xx+float2(dx,dy))*FocalLength+LensCenter);

//	xy.x=xy.x*TextureRect.z+TextureRect.x;
//	xy.y=xy.y*TextureRect.w+TextureRect.y;

    return xy;
    
}