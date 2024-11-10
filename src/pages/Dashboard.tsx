import React from 'react';
import DashboardStats from '../components/DashboardStats';
import LearningProgress from '../components/LearningProgress';
import UpcomingTasks from '../components/UpcomingTasks';

const Dashboard: React.FC = () => {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2">
          <h1 className="text-3xl font-bold mb-6">Welcome back, Dr. Smith</h1>
          <DashboardStats />
          <div className="mt-8">
            <LearningProgress />
          </div>
        </div>
        <div>
          <UpcomingTasks />
        </div>
      </div>
    </div>
  );
};

export default Dashboard;