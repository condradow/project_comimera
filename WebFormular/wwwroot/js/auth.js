export async function login(username, password) {
    const tokenResponse = await fetch(
        "/auth/antiforgery",
        { credentials: "same-origin" }
    );

    if (!tokenResponse.ok)
        throw new Error("Antiforgery-Token konnte nicht geladen werden");

    const { token } = await tokenResponse.json();

    const response = await fetch("/auth/login", {
        method: "POST",
        credentials: "same-origin",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token
        },
        body: JSON.stringify({
            username: username,
            password: password
        })
    });

    if (response.status === 401)
        return false;

    if (!response.ok)
        throw new Error(`Login fehlgeschlagen: ${response.status}`);

    return true;
}