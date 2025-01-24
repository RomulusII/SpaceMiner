using UnityEngine;

public class TeamProjectile : TeamObject {

    public new string LayerName => $"Team{teamID}_Projectiles";
    public new void Start()
    {
        // Takım ID'sine göre merminin Layer'ını ayarla
        gameObject.layer = LayerMask.NameToLayer(LayerName);
    }
}
