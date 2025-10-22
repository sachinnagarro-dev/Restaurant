import { createContext, useContext, useReducer, useEffect, type ReactNode } from 'react';
import { apiService } from '../services/apiService';

interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  user: { username: string; role: string } | null;
  loading: boolean;
}

interface AuthContextType extends AuthState {
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
}

type AuthAction =
  | { type: 'LOGIN_SUCCESS'; payload: { token: string; user: { username: string; role: string } } }
  | { type: 'LOGOUT' }
  | { type: 'SET_LOADING'; payload: boolean };

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'LOGIN_SUCCESS':
      return {
        ...state,
        isAuthenticated: true,
        token: action.payload.token,
        user: action.payload.user,
        loading: false,
      };
    case 'LOGOUT':
      return {
        ...state,
        isAuthenticated: false,
        token: null,
        user: null,
        loading: false,
      };
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    default:
      return state;
  }
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [state, dispatch] = useReducer(authReducer, {
    isAuthenticated: false,
    token: null,
    user: null,
    loading: true,
  });

  useEffect(() => {
    const storedToken = localStorage.getItem('kitchen_jwt_token');
    if (storedToken) {
      // Verify token is still valid
      apiService.setAuthToken(storedToken);
      apiService.getCurrentUser()
        .then(userInfo => {
          dispatch({ type: 'LOGIN_SUCCESS', payload: { token: storedToken, user: userInfo } });
        })
        .catch(() => {
          // Token is invalid, remove it
          localStorage.removeItem('kitchen_jwt_token');
          apiService.clearAuthToken();
          dispatch({ type: 'SET_LOADING', payload: false });
        });
    } else {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, []);

  const login = async (username: string, password: string): Promise<boolean> => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await apiService.loginWithCredentials(username, password);
      localStorage.setItem('kitchen_jwt_token', response.token);
      dispatch({ type: 'LOGIN_SUCCESS', payload: { token: response.token, user: { username, role: response.role } } });
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      dispatch({ type: 'SET_LOADING', payload: false });
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem('kitchen_jwt_token');
    apiService.clearAuthToken();
    dispatch({ type: 'LOGOUT' });
  };

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {!state.loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
