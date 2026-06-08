using UnityEngine;

// Press E near a customer to start their repair mini-game.
public class PlayerInteract : MonoBehaviour
{
    public float interactRadius = 1.2f;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        CustomerAI nearest = FindNearestCustomer();
        if (nearest != null) StartRepair(nearest);
    }

    // Also called by clicking on a customer (CustomerAI.OnMouseDown)
    public void StartRepair(CustomerAI customer)
    {
        Sfx.Click();

        // Buyer customers: fetch the item from the back store, then sell it here.
        if (customer.isBuyer)
        {
            ServeBuyer(customer);
            return;
        }

        string topic = customer.topic;
        KnowledgeSystem.MasteryLevel lvl = GameManager.Instance.knowledge.GetMastery(topic);

        GameManager.Instance.currentCustomerTopic = topic;
        GameManager.Instance.currentCustomerLevel  = (int)lvl;
        GameManager.Instance.lastRepairSuccess     = false;

        string scene = TopicToScene(topic);
        SceneLoader.LoadScene(scene);
    }

    void ServeBuyer(CustomerAI customer)
    {
        var gm = GameManager.Instance;
        if (customer.order == null) customer.order = StockGen.RandomOrder();

        if (gm.carriedItem != null)
        {
            // complete the sale with whatever the player is carrying
            bool ok = gm.carriedItem.Matches(customer.order);
            if (ok)
            {
                gm.AddMoney(customer.order.price);
                gm.AddKnowledge(5);
                ToastUI.Show("SALE COMPLETE!  +$" + customer.order.price + "   +5 KNOWLEDGE", Theme.Green);
            }
            else
            {
                gm.LosePoints(10);
                ToastUI.Show("WRONG SPEC!  -10 KNOWLEDGE  (wanted " + customer.order.ValuesLine() + ")", Theme.Red);
            }
            gm.carriedItem = null;
            gm.activeOrder = null;
            Destroy(customer.gameObject); // customer leaves
        }
        else
        {
            // take the order: show details + stock the back store, and make the
            // customer wait patiently while you fetch it
            gm.activeOrder = customer.order;
            customer.HoldForService();
            if (RoomManager.I != null) RoomManager.I.AcceptOrder(customer.order);
            ToastUI.Show("ORDER: " + customer.order.product + "  " + customer.order.SpecLine()
                         + "\nGo to the BACK STORE (right door) and bring it back.", Theme.Amber);
        }
    }

    CustomerAI FindNearestCustomer()
    {
        CustomerAI[] all = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        CustomerAI best = null;
        float bestDist = interactRadius;
        foreach (var c in all)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }

    static string TopicToScene(string topic)
    {
        return topic switch
        {
            "logic"    => "LogicGate",
            "binary"   => "BinaryDecoder",
            "circuits" => "CircuitTracer",
            "ram"      => "RAMMatcher",
            _          => "LogicGate",
        };
    }
}
