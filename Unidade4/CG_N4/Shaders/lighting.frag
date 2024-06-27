#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float     shininess;
};
struct Light {
    vec3 direction;
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform Light light;
uniform Material material;
uniform vec3 viewPos;
uniform int lightOn;

in vec3 Normal;
in vec3 FragPos;
in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    vec4 outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.2);
    
    //0 = sem iluminacao
    //1 = BasicLighting
    //2 = LightingMaps
    if (lightOn == 1) {
        float ambientStrength = 0.1;
        vec3 ambient = ambientStrength * lightColor;
    
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(lightPos - FragPos);
        
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;
    
        float specularStrength = 0.5;
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32); //The 32 is the shininess of the material.
        vec3 specular = specularStrength * spec * lightColor;
        vec3 result = (ambient + diffuse + specular) * objectColor;
        
        FragColor = vec4(result, 1.0) * outputColor;
    }
    else if (lightOn == 2) {
        vec3 ambient = light.ambient * vec3(texture(material.diffuse, texCoord));
    
        // Diffuse 
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(light.position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, texCoord));
    
        // Specular
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));
        vec3 result = (ambient + diffuse + specular);
        
        FragColor = vec4(result, 1.0) * outputColor;
    }
    else if (lightOn == 3) {
        vec3 ambient = light.ambient * vec3(texture(material.diffuse, texCoord));
    
        // diffuse 
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(-light.direction);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, texCoord));
    
        // specular
        vec3 viewDir = normalize(viewPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));
    
        vec3 result = ambient + diffuse + specular;
        FragColor = vec4(result, 1.0);
    }
    else
        FragColor = outputColor;

    //Note we still use the light color * object color from the last tutorial.
    //This time the light values are in the phong model (ambient, diffuse and specular)
}