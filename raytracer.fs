#version 130   

uniform vec4 eye;
uniform vec4 ambient;
//ambient = (0.1,0.2,0.3,1);
uniform vec4[20] objects;
uniform vec4[20] objColors;
uniform vec4[10] lightsDirection;
uniform vec4[10] lightsIntensity;
uniform vec4[10] lightPosition;
uniform ivec3 sizes; //number of objects & number of lights

in vec3 position1;

vec3 ambientLevel = vec3(0.1,0.2,0.3);
vec3 mirrorAngle;
vec3 intersectionPointOnMirror;
bool mirrorFound = false;

float is_Ray_Sphere(vec4 sphere, vec3 directionV, vec3 viewFrom){
            vec3 f = viewFrom - sphere.xyz;
            float r = sphere.w;
            float a = dot(directionV,directionV);
            float b = dot(2*f,directionV );
            float c = dot(f, f ) - r*r;
            float discriminant = b*b-4*a*c;
            if( discriminant < 0 )
                return -1;
            else{
                discriminant = sqrt( discriminant );
                float t1 = (-b - discriminant)/(2*a);
                float t2 = (-b + discriminant)/(2*a);
                if( t1 >= 0.00001f)
                    return t1;
            }
            return -1;
}

float rayToPlaneDis(vec4 plane, vec3 direction ,vec3 viewFrom){
    float denom = dot(direction,plane.xyz);
    if (abs(denom) > 0.00001f)
    {
        float t = -(plane.w + dot(plane.xyz,viewFrom.xyz)) / denom;
        if (t >= 0.0001f) 
        return t;
    }
    return -1;
}

vec3 colorCalc( vec3 intersectionPoint, vec3 dirV, int mirrorMode){
   vec3 objectColor;
   vec3 normal_to_object;
   vec3 intersectionPointOnScene;
   int found_sphere = -1;
   float notInShadow = 1.0;
   vec3 shadowsAdd = vec3(0,0,0);
   float diffuseLevel = 1.0;
   float closestDist=9999;
   int closestDistObjectIndex= -1;
    
   //go over all the objects
    for (int i = 0 ; i <sizes[0]; i++){ // run on all the objects
        // sphere
        if (objects[i].w>0){ 
            float t =  is_Ray_Sphere(objects[i],(dirV),intersectionPoint); //check if the ray hits the sphere
            if ((t>=0) && (t<closestDist)){
                    closestDist=t;
                    closestDistObjectIndex=i;
            }
        }
    }

    //check if sphere is found and take the closest to screen.
    if (closestDistObjectIndex!=-1){
        found_sphere=closestDistObjectIndex;
        intersectionPointOnScene = (intersectionPoint + closestDist*(dirV));
        objectColor = objColors[closestDistObjectIndex].xyz;
        normal_to_object = (intersectionPointOnScene - objects[closestDistObjectIndex].xyz)/objects[closestDistObjectIndex].w;
    }
    else{
        for (int i = 0 ; i <sizes[0]; i++){
            if (objects[i].w<=0){
                float t = rayToPlaneDis(objects[i],dirV,intersectionPoint);
                if ((t>=0)&&(t<closestDist)){
                        closestDist=t;
                        closestDistObjectIndex=i;
                }
            }
        }

//the mishor is found
    if (closestDistObjectIndex!=-1){
                intersectionPointOnScene = intersectionPoint + (closestDist*(dirV));
                normal_to_object = -objects[closestDistObjectIndex].xyz;
                if (mirrorMode==1){
                    mirrorAngle = (dirV) -2*-normal_to_object*dot((dirV),-normal_to_object);
                    intersectionPointOnMirror = intersectionPointOnScene;
                    if (mirrorFound)
                    return vec3(0.1,1,0.1);
                    mirrorFound = true;
                    
                }
                objectColor = objColors[closestDistObjectIndex].xyz;
                bool check = (intersectionPointOnScene.x>0 && intersectionPointOnScene.y>0) ||(intersectionPointOnScene.x<0 && intersectionPointOnScene.y<0);
                bool modulo = (mod(int(1.5*intersectionPointOnScene.x),2) == mod(int(1.5*intersectionPointOnScene.y),2));
                if ((modulo&&check)||(!modulo&&!check)){
                        diffuseLevel = 0.5;
                }
    }
    else //didnt found any object!
    return vec3(0,0,0);
    }

    //go over all the lights
    for (int i=0;i<sizes[1];i++){
        vec3 spectualarFactor = vec3(0.7,0.7,0.7);
        //for directonal lights
        if (lightsDirection[i].w==0){
        // Calculate the amount of light on this pixel.
        float diffuseFactor = max(dot(-normal_to_object, lightsDirection[i].xyz),0.0);
        //see if pixel is in the shadows
                for (int i2 = 0 ; i2 <sizes[0]; i2++){
                    if ((found_sphere!=i2)&&(objects[i2].w>0)){
                        float t =  is_Ray_Sphere(objects[i2],normalize(-lightsDirection[i].xyz),intersectionPointOnScene); //check if the ray hits the sphere
                        if (t>=0) { //pixel "sun" light is blocked by a sphere
                            notInShadow = notInShadow*0.4;
                            
                        }
                        else if (found_sphere>-1){        
                            vec3 R = normalize(lightsDirection[i].xyz) -2*dot(normalize(lightsDirection[i].xyz),-normal_to_object)*-normal_to_object;
                            vec3 V = normalize( intersectionPoint - intersectionPointOnScene );
                            float scalar = clamp(max(0, dot(R,V)),0.0f,1.0f);
                            vec3 Specular = spectualarFactor.xyz * pow(scalar , 50) *lightsIntensity[i].xyz;        
                            shadowsAdd += Specular;
                        }
                    }
                }
            shadowsAdd += diffuseLevel*diffuseFactor*objectColor*notInShadow*lightsIntensity[i].xyz;
        }

//spotlight 
        else{
                vec3 lightColor = lightsIntensity[i].xyz;
                vec3 spotDirection = normalize(lightsDirection[i].xyz);
                float spotCosineCutoff = lightPosition[i].w; // = 0.6
                float spotExponent = 2.0f;  // to calc
                vec3 L = normalize(intersectionPointOnScene -lightPosition[i].xyz);
            
                float spotCosine = dot(normalize(spotDirection),L);
                if (spotCosine >= spotCosineCutoff) { // The point is the cone
                    float spotFactor = pow(spotCosine,spotExponent);
                    for (int i2 = 0 ; i2 <sizes[0]; i2++){
                        if ((found_sphere!=i2)&&(objects[i2].w>0)){
                            float t =  is_Ray_Sphere(objects[i2],-L,intersectionPointOnScene); //check if the ray hits the sphere
                            if (t>=0) { //specular is blocked by a sphere
                                spotFactor = 0;
                            }
                            else if (found_sphere>-1){
                                vec3 R = L -2*dot(L,-normal_to_object)*-normal_to_object;
                                vec3 V = normalize( intersectionPoint - intersectionPointOnScene );
                                float scalar = clamp(max(0, dot(R,V)),0.0f,1.0f);
                                vec3 Specular = spectualarFactor.xyz * pow(scalar , 40) *lightsIntensity[i].xyz;            
                                shadowsAdd += Specular;
                            }
                        }
                    }
                shadowsAdd += diffuseLevel*spotFactor*objectColor*notInShadow*lightsIntensity[i].xyz;
                } 
            }
    }
        return ambient.xyz*objectColor+shadowsAdd;
}

vec3 colorCalc( vec3 intersectionPoint){
   int mirror;
   if (eye.w==0)
   mirror = 1;
   else
   mirror = 0;
   vec3 directionVector = position1 - intersectionPoint;
   vec3 color = colorCalc(intersectionPoint, directionVector,mirror);
   if (mirrorFound)
    return colorCalc (intersectionPointOnMirror,mirrorAngle,mirror);
   else   
    return color;
  
}

void main()
{  
            gl_FragColor = vec4(colorCalc(eye.xyz),1);      
}
 

