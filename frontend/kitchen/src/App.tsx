import type { FC } from 'react'
import { useState } from 'react'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { KitchenProvider } from './contexts/KitchenContext'
import KitchenScreen from './components/KitchenScreen'
import LoginScreen from './components/LoginScreen'

const AppContent: FC = () => {
  const { isAuthenticated } = useAuth();
  const [showLogin, setShowLogin] = useState(!isAuthenticated);

  if (!isAuthenticated || showLogin) {
    return <LoginScreen onLoginSuccess={() => setShowLogin(false)} />;
  }

  return (
    <KitchenProvider>
      <KitchenScreen />
    </KitchenProvider>
  );
};

const App: FC = () => {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
};

export default App;
