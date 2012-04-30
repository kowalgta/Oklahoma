namespace Oklahoma
{
    public enum SaleCycleCookieBehavior : short
    {
        // Will use cookies and requires SessionId
        CookiesAreEnabledAndSessionIdIsRequired = 0,
        // Session will be handled by SaleCycle - SessionId is not required
        SaleCycleHandlesSessionManagement = 1,
        // SaleCycle will not store any cookies on client's machine
        DoNotStoreCookies = 2
    }
}