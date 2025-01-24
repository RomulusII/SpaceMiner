using UnityEngine;

public class TeamObject : MonoBehaviour
{
    public int teamID;

    public string LayerName => $"Team{teamID}_Ships";

    public void Start()
    {
        // Takım ID'sine göre geminin Layer'ını ayarla
        gameObject.layer = LayerMask.NameToLayer(LayerName);
    }

    public void SetTeam(int teamId)
    {
        teamID = teamId;
        gameObject.layer = LayerMask.NameToLayer(LayerName);
    }
}
