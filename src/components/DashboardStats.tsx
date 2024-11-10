import React from 'react';

const DashboardStats = () => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold text-gray-600">Completed Modules</h3>
        <p className="text-3xl font-bold text-primary-600">12/20</p>
        <div className="mt-2 h-2 bg-gray-200 rounded">
          <div className="h-full bg-primary-600 rounded" style={{ width: '60%' }}></div>
        </div>
      </div>
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold text-gray-600">Assessment Score</h3>
        <p className="text-3xl font-bold text-primary-600">85%</p>
        <p className="text-sm text-gray-500 mt-1">Last assessment: 2 days ago</p>
      </div>
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold text-gray-600">Study Time</h3>
        <p className="text-3xl font-bold text-primary-600">24h</p>
        <p className="text-sm text-gray-500 mt-1">This week</p>
      </div>
    </div>
  );
};

export default DashboardStats;