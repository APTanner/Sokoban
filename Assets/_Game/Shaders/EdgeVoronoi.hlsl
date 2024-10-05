//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

inline float2 unity_voronoi_noise_randomVector (float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

void Edge_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Edge)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    
    float closestDistance = 8.0;
    float secondDistance = 0.0;
    
    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < closestDistance)
            {
                secondDistance = closestDistance;
                closestDistance = d;
            }
            else 
            {
                secondDistance = min(secondDistance, d);    
            }
        }
    }

    Out = closestDistance;
    Edge = secondDistance - closestDistance;
}
#endif //MYHLSLINCLUDE_INCLUDED