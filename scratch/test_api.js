async function run() {
  try {
    // 1. Login to get token
    console.log('Logging in...');
    const loginRes = await fetch('http://localhost:5000/api/v1/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: 'admin@realsync.vn',
        password: 'Admin@123'
      })
    });
    
    const loginData = await loginRes.json();
    if (!loginRes.ok) {
      console.error('Login failed:', loginData);
      return;
    }
    
    const token = loginData.data.accessToken;
    console.log('Login successful! Token acquired.');

    // 2. Try to create the connected account
    console.log('Creating connected account...');
    const payload = {
      provider: "Facebook",
      channelType: "FacebookGroup",
      displayName: "Nhóm BĐS của tôi",
      externalAccountId: "1687736462275874",
      externalParentAccountId: null,
      profileUrl: "https://www.facebook.com/groups/1687736462275874",
      avatarUrl: null,
      accessToken: "local-stealth-session",
      refreshToken: null,
      expiresInSeconds: null,
      grantedScopesJson: null
    };

    const res = await fetch('http://localhost:5000/api/v1/connectedaccounts', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(payload)
    });

    const resData = await res.json();
    console.log('Status:', res.status);
    console.log('Response data:', JSON.stringify(resData, null, 2));
  } catch (err) {
    console.error('Error occurred:', err.message);
  }
}

run();
