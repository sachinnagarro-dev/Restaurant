# Kitchen Management System

A React + TypeScript kitchen dashboard for managing restaurant orders in real-time. Built with Vite, Tailwind CSS, and SignalR for real-time updates.

## Features

### 🍽️ **Real-time Order Management**
- Live order updates via SignalR connection to OrderHub
- Automatic status synchronization across kitchen displays
- Connection status indicator with auto-reconnect

### 📋 **Order Tracking**
- **Order Cards**: Display order ID, table number, items, time, and status
- **Status Progression**: Received → Preparing → Ready → Served → Closed
- **Quick Actions**: One-click status updates with visual feedback
- **Order Details**: Special instructions, item notes, and totals

### 🔍 **Filtering & Search**
- **Status Filters**: Filter by order status (All, Received, Preparing, Ready, Served, Closed)
- **Search**: Find orders by order ID, table number, or item name
- **Quick Stats**: Live counts of orders by status
- **Real-time Updates**: Filters update automatically as orders change

### 🖨️ **Kitchen Ticket Printing**
- **Print Functionality**: `window.print()` integration for kitchen tickets
- **Formatted Tickets**: Clean, printable layout with order details
- **Print Styles**: Optimized CSS for thermal printers
- **Order Information**: Table, time, items, special instructions, totals

### 🎨 **Modern UI**
- **Tailwind CSS**: Minimal, responsive design
- **Status Colors**: Color-coded order status indicators
- **Responsive Grid**: Adapts to different screen sizes
- **Touch-Friendly**: Large buttons and touch targets for kitchen use

## Tech Stack

- **React 19** with TypeScript
- **Vite** for fast development and building
- **Tailwind CSS** for styling
- **SignalR** for real-time communication
- **React Context** for state management

## Development

### Prerequisites
- Node.js 18+
- Backend API running on port 5000

### Setup
```bash
cd frontend/kitchen
npm install
npm run dev
```

### Scripts
- `npm run dev` - Start development server (port 3001)
- `npm run build` - Build for production
- `npm run preview` - Preview production build

### API Integration
The app connects to the backend via proxy:
- `/api` → `http://localhost:5000` (REST API)
- `/orderHub` → `http://localhost:5000` (SignalR WebSocket)

## Usage

### Kitchen Workflow
1. **Orders Arrive**: New orders appear automatically via SignalR
2. **Start Preparing**: Click "Start Preparing" to begin cooking
3. **Mark Ready**: Click "Mark Ready" when food is prepared
4. **Mark Served**: Click "Mark Served" when delivered to table
5. **Close Order**: Click "Close Order" to complete

### Printing Tickets
- Click the 🖨️ **Print** button on any order card
- Browser print dialog opens with formatted kitchen ticket
- Optimized for thermal printers (58mm/80mm)

### Filtering Orders
- Use status buttons to filter by order status
- Search by order ID, table number, or item name
- View live statistics in the filter bar

## Components

### `KitchenScreen`
Main dashboard component with header, filters, and order grid.

### `OrderCard`
Individual order display with status, items, and action buttons.

### `KitchenFilters`
Search and filter controls with live statistics.

### `ConnectionStatus`
Real-time connection indicator for SignalR.

## State Management

### `KitchenContext`
React Context for global state management:
- **Orders**: Array of all orders
- **Filters**: Search term and status filter
- **Connection**: SignalR connection status
- **Loading**: API request states

### Actions
- `loadOrders()` - Fetch orders from API
- `updateOrderStatus()` - Update order status
- `setFilters()` - Update search/filter state
- `printKitchenTicket()` - Open print dialog

## Styling

### Tailwind Classes
- **Custom Colors**: Kitchen-themed color palette
- **Status Badges**: Color-coded order status
- **Responsive Grid**: Auto-adjusting layout
- **Print Styles**: Optimized for thermal printers

### Print Styles
```css
@media print {
  .kitchen-ticket {
    border: 2px solid #000;
    padding: 20px;
    max-width: 400px;
  }
}
```

## Deployment

### Production Build
```bash
npm run build
```

### Environment Variables
- `VITE_API_URL` - Backend API URL (default: proxy to localhost:5000)
- `VITE_SIGNALR_URL` - SignalR Hub URL (default: proxy to localhost:5000)

## Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## License
MIT