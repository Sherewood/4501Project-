using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents a technology itself
public struct Technology
{
    public Technology(string id, string name, string description, int cost)
    {
        Id = id;
        Name = name;
        Description = description;
        Cost = cost;
    }
    //id of the technology (used internally) 
    public string Id;
    //name of the technology (used for display)
    public string Name;
    //description of the technology
    public string Description;
    //cost in research points
    public int Cost;
}

//stores a specific technology in a tree structure
public class TechnologyNode
{
    //prior technologies required to research this technology
    private List<TechnologyNode> _prerequisites;
    //technologies that require this technology to be researched
    private List<TechnologyNode> _children;

    private Technology _technology;

    //true if technology completed, false otherwise
    private bool _completed;

    public TechnologyNode(string id, string name, string description, int cost)
    {
        _technology = new Technology(id, name, description, cost);
        _prerequisites = new List<TechnologyNode>();
        _children = new List<TechnologyNode>();
        _completed = false;
    }

    public bool IsCompleted()
    {
        return _completed;
    }

    public void Complete()
    {
        _completed = true;
    }

    //get technology struct stored within node
    public Technology GetTechnology()
    {
        return _technology;
    }

    public void AddPrerequisite(TechnologyNode prerequisite)
    {
        if (!_prerequisites.Contains(prerequisite))
        {
            _prerequisites.Add(prerequisite);

            //link the prerequisite to this technology aswell
            prerequisite.AddChild(this);
        }
    }

    public void AddChild(TechnologyNode child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
        }
    }

    public List<TechnologyNode> GetChildren()
    {
        return _children;
    }

    //returns true if the technology has no prerequisities
    public bool IsRootOfTree()
    {
        return (_prerequisites.Count == 0);
    }

    //returns true if all prerequisite technologies are researched, and tech is not completed
    public bool CanResearchTechnology()
    {
        if (IsCompleted())
        {
            return false;
        }

        foreach(TechnologyNode prereq in _prerequisites)
        {
            if (!prereq.IsCompleted())
            {
                return false;
            }
        }
        return true;
    }

    //returns true if the given amount of research points is enough to afford the technology
    public bool CanAffordResearch(int rp)
    {
        return (rp >= _technology.Cost);
    }
}

/* Model class */
//Purpose: Handle tracking of available technologies.
public class ResearchModel : MonoBehaviour
{
    //all technology nodes in the tech tree, for fast access
    //format (techID, techNode)
    private Dictionary<string, TechnologyNode> _technologyNodes;

    //all root nodes in the tech tree, for efficient search
    private List<TechnologyNode> _techTreeRoots;

    // Init all available technologies
    // Once again, file-based much better, and no excuse not to do it at this point, but also no :(
    void Awake()
    {
        _technologyNodes = new Dictionary<string, TechnologyNode>();
        _techTreeRoots = new List<TechnologyNode>();

        /* Add technologies here */
        /*
         Use 'Tech_' prefix before every tech id
        
                     | tech id | tech name | tech cost | tech description | prerequisite technologies |
        */
        AddTechnology("Tech_AdvMilitary","Advanced Military Research", 750, "Paves the way for new breakthroughs in military tech.", new List<string>() { });
        AddTechnology("Tech_HyperBoost","Hyper Boost", 1000, "Unlocks a new special power for your infantry.", new List<string>() { "Tech_AdvMilitary" });
        AddTechnology("Tech_Plant","Plant", 1000, "Unlocks a new special power for your vehicles.", new List<string>() { "Tech_AdvMilitary" });

        DetermineRootNodes();
    }

    //helper for adding technology
    //requires adding prerequisite technologies before the technology that will trigger them.
    private void AddTechnology(string techId, string techName, int techCost, string description, List<string> prerequisities)
    {
        TechnologyNode newTech = new TechnologyNode(techId, techName, description, techCost);

        foreach(string prereq in prerequisities)
        {
            TechnologyNode prereqTech = GetTechnology(prereq);
            if (prereqTech == null)
            {
                Debug.LogError("Research initialization failed - tried to add technology '" + techId + "' before its prerequisite '" + prereq + "'!");
                return;
            }

            newTech.AddPrerequisite(prereqTech);
        }

        _technologyNodes.Add(techId, newTech);
    }

    //determine which of the technologies added have no prerequisites, and track them
    //call only after all technologies have been initiailized
    private void DetermineRootNodes()
    {
        foreach(TechnologyNode tech in _technologyNodes.Values)
        {
            if (tech.IsRootOfTree())
            {
                _techTreeRoots.Add(tech);
            }
        }
    }

    private TechnologyNode GetTechnology(string techId)
    {
        if (_technologyNodes.ContainsKey(techId))
        {
            return _technologyNodes[techId];
        }
        return null;
    }

    /* public methods */

    //returns cost of tech
    public int GetTechnologyCost(string techId)
    {
        TechnologyNode candidateTech = GetTechnology(techId);
        if (candidateTech == null)
        {
            Debug.LogError("Tried to determine if technology that doesn't exist can be researched: '" + techId + "'");
            return 0;
        }

        //get the cost and return it
        return candidateTech.GetTechnology().Cost;
    }

    //tries to complete the technology of the specified name
    //returns true if successful, or false if the tech is already completed
    public bool CompleteTechnology(string techId)
    {
        TechnologyNode completedTech = GetTechnology(techId);
        if (completedTech == null)
        {
            Debug.LogError("Tried to complete technology that doesn't exist: '" + techId + "'");
            return false;
        }

        if (completedTech.IsCompleted())
        {
            Debug.LogWarning("Tried to complete technology that was already completed: '" + techId + "'");
            return false;
        }

        completedTech.Complete();
        return true;
    }

    //returns true if technology is researched
    public bool IsTechResearched(string techId)
    {
        TechnologyNode candidateTech = GetTechnology(techId);
        if (candidateTech == null)
        {
            Debug.LogError("Tried to find technology that doesn't exist: '" + techId + "'");
            return false;
        }

        return candidateTech.IsCompleted();
    }

    //returns true if technology is researchable given the current research points, false otherwise
    //means COMPLETE the research, not just unlocked
    public bool CanTechBeResearched(string techId, int rp)
    {
        TechnologyNode candidateTech = GetTechnology(techId);
        if (candidateTech == null)
        {
            Debug.LogError("Tried to determine if technology that doesn't exist can be researched: '" + techId + "'");
            return false;
        }

        //true only if all prereqs completed, and enough research points available
        return candidateTech.CanResearchTechnology() && candidateTech.CanAffordResearch(rp); 
    }

    //gets all technologies that are available to be researched
    public List<Technology> GetResearchableTechnologies()
    {
        List<Technology> researchableTechs = new List<Technology>();

        //perform BFS to find all researchable techs in tree
        Queue<TechnologyNode> reached = new Queue<TechnologyNode>();
        List<TechnologyNode> visited = new List<TechnologyNode>();

        foreach(TechnologyNode rootTechNode in _techTreeRoots)
        {
            if (rootTechNode.CanResearchTechnology())
            {
                researchableTechs.Add(rootTechNode.GetTechnology());
            }
            else
            {
                reached.Enqueue(rootTechNode);
                visited.Add(rootTechNode);
            }
        }

        //while there are tech nodes to check
        while (reached.Count > 0)
        {
            TechnologyNode current = reached.Dequeue();

            List<TechnologyNode> currentChildren = current.GetChildren();
            //check each child to see if it is researchable, else add it to the list of nodes to check next
            foreach(TechnologyNode child in currentChildren)
            {
                if (visited.Contains(child))
                {
                    continue;
                }

                if (child.CanResearchTechnology())
                {
                    researchableTechs.Add(child.GetTechnology());
                }
                else
                {
                    reached.Enqueue(child);
                    visited.Add(child);
                }
            }
        }

        return researchableTechs;
    }

}
