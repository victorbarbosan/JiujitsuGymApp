namespace JiujitsuGymApp.Models
{
    /// <summary>
    /// Mirrors Stripe's subscription status values so we can store, in our own DB,
    /// a cached copy of what Stripe considers the subscription state to be.
    /// See: https://docs.stripe.com/api/subscriptions/object#subscription_object-status
    /// </summary>
    public enum SubscriptionStatus
    {
        Incomplete = 0,
        IncompleteExpired = 1,
        Trialing = 2,
        Active = 3,
        PastDue = 4,
        Canceled = 5,
        Unpaid = 6,
        Paused = 7
    }
}
